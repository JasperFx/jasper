using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Transports;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class MartenBackedMessagePersistenceTests : PostgresqlContext, IDisposable, IAsyncLifetime
    {
        public MartenBackedMessagePersistenceTests()
        {
            theHost = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                }).IntegrateWithJasper();

            });



            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);
            theEnvelope.Status = EnvelopeStatus.Scheduled;
            theEnvelope.MessageType = "message1";
            theEnvelope.ContentType = EnvelopeConstants.JsonContentType;
        }

        public async Task InitializeAsync()
        {
            var persistence = theHost.Get<IEnvelopePersistence>();

            await persistence.Admin.RebuildStorageAsync();


            persistence.ScheduleJobAsync(theEnvelope).Wait(3.Seconds());

            persisted = (await persistence.Admin
                .AllIncomingEnvelopes())

                .FirstOrDefault(x => x.Id == theEnvelope.Id);
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            theHost.Dispose();
        }

        private readonly IHost theHost;
        private readonly Envelope theEnvelope;
        private Envelope persisted;

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
