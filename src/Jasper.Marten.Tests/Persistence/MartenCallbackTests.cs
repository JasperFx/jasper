using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.WorkerQueues;
using Jasper.Marten.Persistence;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Bus;
using Marten;
using Marten.Schema;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{
    public class MartenCallbackTests : IDisposable
    {
        private JasperRuntime theRuntime;
        private IDocumentStore theStore;
        private Envelope theEnvelope;
        private MartenCallback theCallback;

        public MartenCallbackTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
            });

            theStore = theRuntime.Get<IDocumentStore>();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Status = TransportConstants.Incoming;

            theStore.BulkInsert(new Envelope[]{theEnvelope});

            theCallback = new MartenCallback(theEnvelope, Substitute.For<IWorkerQueue>(), theStore);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public async Task mark_complete_deletes_the_envelope()
        {
            await theCallback.MarkComplete();

            using (var session = theStore.QuerySession())
            {
                SpecificationExtensions.ShouldBeNull((await session.LoadAsync<Envelope>(theEnvelope.Id)));
            }


        }

        [Fact]
        public async Task move_to_errors_persists_the_error_report()
        {
            await theCallback.MoveToErrors(theEnvelope, new Exception("Boom!"));

            using (var session = theStore.QuerySession())
            {
                (await session.LoadAsync<Envelope>(theEnvelope.Id)).ShouldBeNull();


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
                var persisted = await session.LoadAsync<Envelope>(theEnvelope.Id);

                persisted.Attempts.ShouldBe(1);
            }
        }

        [Fact]
        public async Task move_to_delayed_until()
        {
            var time = DateTime.Today.ToUniversalTime().AddDays(1);

            await theCallback.MoveToDelayedUntil(time, theEnvelope);

            using (var session = theStore.QuerySession())
            {
                var persisted = await session.LoadAsync<Envelope>(theEnvelope.Id);
                persisted.Status.ShouldBe(TransportConstants.Scheduled);
                persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
                persisted.ExecutionTime.ShouldBe(time);
            }
        }
    }
}
