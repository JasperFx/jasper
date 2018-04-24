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
using Jasper.SqlServer.Persistence;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class SqlServerCallbackTests : IDisposable
    {
        private JasperRuntime theRuntime;
        private Envelope theEnvelope;
        private DurableCallback theCallback;
        private EnvelopeRetries theRetries;
        private SqlServerEnvelopePersistor thePersistor;

        public SqlServerCallbackTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString);
            });

            theRuntime.RebuildMessageStorage();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Status = TransportConstants.Incoming;



            thePersistor = theRuntime.Get<SqlServerEnvelopePersistor>();
            thePersistor.StoreIncoming(theEnvelope).Wait(3.Seconds());


            var logger = TransportLogger.Empty();
            theRetries = new EnvelopeRetries(thePersistor, logger, new MessagingSettings());



            theCallback = new DurableCallback(theEnvelope, Substitute.For<IWorkerQueue>(), thePersistor, theRetries);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public async Task mark_complete_deletes_the_envelope()
        {
            await theCallback.MarkComplete();

            theRetries.IncomingDeleted.WaitOne(500);

            var persisted = thePersistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);


            persisted.ShouldBeNull();

        }

        [Fact]
        public async Task move_to_errors_persists_the_error_report()
        {
            await theCallback.MoveToErrors(theEnvelope, new Exception("Boom!"));

            theRetries.ErrorReportLogged.WaitOne(500);

            var persisted = thePersistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);


            persisted.ShouldBeNull();


            var report = await thePersistor.LoadDeadLetterEnvelope(theEnvelope.Id);

            report.ExceptionMessage.ShouldBe("Boom!");


        }

        [Fact]
        public async Task can_reload_the_error_report()
        {
            await theCallback.MoveToErrors(theEnvelope, new Exception("Boom!"));

            theRetries.ErrorReportLogged.WaitOne(500);




            var report = await thePersistor.LoadDeadLetterEnvelope(theEnvelope.Id);

            report.ExceptionMessage.ShouldBe("Boom!");
        }

        [Fact]
        public async Task requeue()
        {
            await theCallback.Requeue(theEnvelope);

            var persisted = thePersistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);

            persisted.ShouldNotBeNull();
        }

        [Fact]
        public async Task move_to_delayed_until()
        {
            var time = DateTime.Today.ToUniversalTime().AddDays(1);

            await theCallback.MoveToScheduledUntil(time, theEnvelope);

            theRetries.Scheduled.WaitOne(1.Seconds());

            var persisted = thePersistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
            persisted.Status.ShouldBe(TransportConstants.Scheduled);
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
            persisted.ExecutionTime.ShouldBe(time);
        }
    }
}
