using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Transports;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    [MessageIdentity("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }


    public class SqlServerBackedMessagePersistenceTests : SqlServerContext, IAsyncLifetime
    {
        public SqlServerBackedMessagePersistenceTests()
        {

        }

        protected override async Task initialize()
        {
            theHost = JasperHost.For(opts =>
            {
                opts.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
            });

            await theHost.RebuildMessageStorage();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theHost.Get<IEnvelopePersistence>().ScheduleJobAsync(theEnvelope).Wait(3.Seconds());

            var persistor = theHost.Get<SqlServerEnvelopePersistence>();

            persisted = (await persistor.Admin.AllIncomingEnvelopes())
                .FirstOrDefault(x => x.Id == theEnvelope.Id);
        }

        public override Task DisposeAsync()
        {
            return theHost.StopAsync();
        }

        private IHost theHost;
        private Envelope theEnvelope;
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
