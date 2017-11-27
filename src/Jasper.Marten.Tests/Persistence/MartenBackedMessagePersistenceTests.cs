using System;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Marten.Persistence;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Bus;
using Marten;
using Xunit;
using Shouldly;

namespace Jasper.Marten.Tests.Persistence
{
    public class MartenBackedMessagePersistenceTests : IDisposable
    {
        private JasperRuntime theRuntime;
        private Envelope theEnvelope;
        private Envelope persisted;

        public MartenBackedMessagePersistenceTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
            });

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theRuntime.Get<MartenBackedMessagePersistence>().ScheduleMessage(theEnvelope).Wait(3.Seconds());

            using (var session = theRuntime.Get<IDocumentStore>().LightweightSession())
            {
                persisted = session.Load<Envelope>(theEnvelope.Id);
            }
        }

        [Fact]
        public void should_persist_the_scheduled_envelope()
        {
            persisted.ShouldNotBeNull();
        }

        [Fact]
        public void should_be_in_scheduled_status()
        {
            persisted.Status.ShouldBe(TransportConstants.Scheduled);
        }

        [Fact]
        public void should_be_owned_by_any_node()
        {
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }
    }
}
