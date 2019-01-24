using System;
using System.Linq;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer.Persistence
{
    [MessageIdentity("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }


    public class SqlServerBackedMessagePersistenceTests : SqlServerContext, IDisposable
    {
        public SqlServerBackedMessagePersistenceTests()
        {
            theHost = JasperHost.For(_ =>
            {
                _.Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
            });

            theHost.RebuildMessageStorage();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theHost.Get<SqlServerBackedDurableMessagingFactory>().ScheduleJob(theEnvelope).Wait(3.Seconds());

            var persistor = theHost.Get<SqlServerEnvelopePersistor>();

            persisted = persistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
        }

        public void Dispose()
        {
            theHost.Dispose();
        }

        private readonly IJasperHost theHost;
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
