using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Persistence;
using Jasper.Persistence.Marten.Persistence.DbObjects;
using Jasper.Persistence.Marten.Persistence.Operations;
using Marten;
using NSubstitute;
using Servers;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{
    public class MartenCallbackTests : IDisposable
    {
        public MartenCallbackTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(MartenContainer.ConnectionString);

                _.ConfigureMarten(x =>
                {
                    x.Storage.Add<PostgresqlEnvelopeStorage>();
                    x.PLV8Enabled = false;
                });
            });

            theStore = theRuntime.Get<IDocumentStore>();

            theStore.Advanced.Clean.CompletelyRemoveAll();
            theStore.Schema.ApplyAllConfiguredChangesToDatabase();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Status = TransportConstants.Incoming;

            var marker = new EnvelopeTables(new MessagingSettings(), new StoreOptions());

            using (var session = theStore.OpenSession())
            {
                session.StoreIncoming(marker, theEnvelope);
                session.SaveChanges();
            }


            var logger = TransportLogger.Empty();
            theRetries = new EnvelopeRetries(new MartenEnvelopePersistor(theStore, marker), logger,
                new MessagingSettings());


            var persistor = new MartenEnvelopePersistor(theStore, marker);

            theCallback =
                new DurableCallback(theEnvelope, Substitute.For<IWorkerQueue>(), persistor, theRetries, logger);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        private readonly JasperRuntime theRuntime;
        private readonly IDocumentStore theStore;
        private readonly Envelope theEnvelope;
        private readonly DurableCallback theCallback;
        private readonly EnvelopeRetries theRetries;

        [Fact]
        public async Task can_reload_the_error_report()
        {
            await theCallback.MoveToErrors(theEnvelope, new Exception("Boom!"));

            theRetries.ErrorReportLogged.WaitOne(500);


            var persistence = theRuntime.Get<MartenEnvelopePersistor>();

            var report = await persistence.LoadDeadLetterEnvelope(theEnvelope.Id);

            report.ExceptionMessage.ShouldBe("Boom!");
        }

        [Fact]
        public async Task mark_complete_deletes_the_envelope()
        {
            await theCallback.MarkComplete();

            theRetries.IncomingDeleted.WaitOne(500);

            using (var session = theStore.QuerySession())
            {
                var persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);


                persisted.ShouldBeNull();
            }
        }

        [Fact]
        public async Task move_to_delayed_until()
        {
            var time = DateTime.Today.ToUniversalTime().AddDays(1);

            await theCallback.MoveToScheduledUntil(time, theEnvelope);

            theRetries.Scheduled.WaitOne(1.Seconds());

            using (var session = theStore.QuerySession())
            {
                var persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
                persisted.Status.ShouldBe(TransportConstants.Scheduled);
                persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
                persisted.ExecutionTime.ShouldBe(time);
            }
        }

        [Fact]
        public async Task move_to_errors_persists_the_error_report()
        {
            await theCallback.MoveToErrors(theEnvelope, new Exception("Boom!"));

            theRetries.ErrorReportLogged.WaitOne(500);

            using (var session = theStore.QuerySession())
            {
                var persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);


                persisted.ShouldBeNull();


                var report = await session.LoadAsync<ErrorReport>(theEnvelope.Id);

                report.ExceptionMessage.ShouldBe("Boom!");
            }
        }

        [Fact]
        public async Task requeue()
        {
            await theCallback.Requeue(theEnvelope);

            using (var session = theStore.QuerySession())
            {
                var persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);

                persisted.ShouldNotBeNull();
            }
        }
    }
}
