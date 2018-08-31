using System;
using System.Linq;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Persistence;
using Jasper.Persistence.Marten.Persistence.DbObjects;
using Jasper.Persistence.Marten.Persistence.Operations;
using Marten;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten.Persistence
{
    public class MartenBackedMessagePersistenceTests : MartenContext, IDisposable
    {
        public MartenBackedMessagePersistenceTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(Servers.PostgresConnectionString);
                _.ConfigureMarten(x =>
                {
                    x.Storage.Add<PostgresqlEnvelopeStorage>();
                    x.PLV8Enabled = false;
                });
            });

            theRuntime.Get<IDocumentStore>().Schema.ApplyAllConfiguredChangesToDatabase();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theRuntime.Get<MartenBackedDurableMessagingFactory>().ScheduleJob(theEnvelope).Wait(3.Seconds());


            using (var session = theRuntime.Get<IDocumentStore>().LightweightSession())
            {
                persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
            }
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }

        private readonly JasperRuntime theRuntime;
        private readonly Envelope theEnvelope;
        private readonly Envelope persisted;

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

        [Fact]
        public void should_persist_the_scheduled_envelope()
        {
            persisted.ShouldNotBeNull();
        }
    }
}
