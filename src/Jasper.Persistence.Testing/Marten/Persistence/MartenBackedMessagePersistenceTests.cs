using System;
using System.Linq;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Transports;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class MartenBackedMessagePersistenceTests : PostgresqlContext, IDisposable
    {
        public MartenBackedMessagePersistenceTests()
        {
            theHost = JasperHost.For(_ =>
            {
                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                });

            });



            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);
            theEnvelope.Status = EnvelopeStatus.Scheduled;

            var persistence = theHost.Get<IEnvelopePersistence>();

            persistence.Admin.RebuildSchemaObjects().GetAwaiter().GetResult();


            persistence.ScheduleJob(theEnvelope).Wait(3.Seconds());

            persisted = persistence.Admin
                .AllIncomingEnvelopes()
                .GetAwaiter()
                .GetResult()
                .FirstOrDefault(x => x.Id == theEnvelope.Id);

        }

        public void Dispose()
        {
            theHost.Dispose();
        }

        private readonly IHost theHost;
        private readonly Envelope theEnvelope;
        private readonly Envelope persisted;

        [Fact]
        public void should_be_in_scheduled_status()
        {
            persisted.Status.ShouldBe(EnvelopeStatus.Scheduled);
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
