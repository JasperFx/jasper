using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.Testing.SqlServer.Durability.App;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.Durability
{
    [Collection("sqlserver")]
    public class SqlServerBackedPersistenceFixture : IAsyncLifetime
    {
        private LightweightCache<string, IHost> _receivers;

        private LightweightCache<string, IHost> _senders;
        private SenderLatchDetected _senderWatcher;

        public async Task InitializeAsync()
        {

            _senderWatcher = new SenderLatchDetected(new LoggerFactory());


            var receiverAdmin = new SqlServerEnvelopeStorageAdmin(new SqlServerSettings
                {ConnectionString = Servers.SqlServerConnectionString, SchemaName = "receiver"});

            await receiverAdmin.RebuildSchemaObjects();


            var senderAdmin = new SqlServerEnvelopeStorageAdmin(new SqlServerSettings
                {ConnectionString = Servers.SqlServerConnectionString, SchemaName = "sender"});
            await senderAdmin.RebuildSchemaObjects();

            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                await CommandExtensions.CreateCommand(conn, @"
IF OBJECT_ID('receiver.trace_doc', 'U') IS NOT NULL
  drop table receiver.trace_doc;

").ExecuteNonQueryAsync();

                await CommandExtensions.CreateCommand(conn, @"
create table receiver.trace_doc
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQueryAsync();
            }

            _receivers = new LightweightCache<string, IHost>(key =>
            {
                var registry = new ReceiverApp();

                return JasperHost.For(registry);
            });

            _senders = new LightweightCache<string, IHost>(key =>
            {
                var registry = new SenderApp();

                registry.Services.For<ITransportLogger>().Use(_senderWatcher);

                return JasperHost.For(registry);
            });
        }

        public async Task DisposeAsync()
        {
            foreach (var receiver in _receivers)
            {
                await receiver.StopAsync();
            }
            _receivers.ClearAll();

            foreach (var sender in _senders)
            {
                await sender.StopAsync();
            }
            _senders.ClearAll();

        }

        public void StartReceiver(string name)
        {
            _receivers.FillDefault(name);
        }

        public void StartSender(string name)
        {
            _senderWatcher.Reset();
            _senders.FillDefault(name);
        }

        public async Task SendMessages(string sender, int count)
        {
            var runtime = _senders[sender];

            for (var i = 0; i < count; i++)
            {
                var msg = new TraceMessage {Name = Guid.NewGuid().ToString()};
                await runtime.Send(msg);
            }
        }

        public int ReceivedMessageCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();
                return (int) CommandExtensions.CreateCommand(conn, "select count(*) from receiver.trace_doc").ExecuteScalar();
            }
        }


        public async Task WaitForMessagesToBeProcessed(int count)
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                await conn.OpenAsync();

                for (var i = 0; i < 200; i++)
                {
                    var actual = (int) await CommandExtensions.CreateCommand(conn, "select count(*) from receiver.trace_doc").ExecuteScalarAsync();


                    var envelopeCount = PersistedIncomingCount();


                    Trace.WriteLine($"waitForMessages: {actual} actual & {envelopeCount} incoming envelopes");

                    if (actual == count && envelopeCount == 0) return;

                    await Task.Delay(250);
                }
            }


            throw new Exception("All messages were not received");
        }

        public int PersistedIncomingCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                return (int) CommandExtensions.CreateCommand(conn, $"select count(*) from receiver.{DatabaseConstants.IncomingTable}")
                    .ExecuteScalar();
            }
        }

        public int PersistedOutgoingCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                return (int) CommandExtensions.CreateCommand(conn, $"select count(*) from sender.{DatabaseConstants.OutgoingTable}")
                    .ExecuteScalar();
            }
        }

        public async Task StopReceiver(string name)
        {
            var receiver = _receivers[name];
            await receiver.StopAsync();
            receiver.Dispose();
            _receivers.Remove(name);
        }

        public async Task StopSender(string name)
        {
            var sender = _senders[name];
            await sender.StopAsync();
            sender.Dispose();
            _senders.Remove(name);
        }

        [Fact]
        public async Task Sending_Recovered_Messages_when_Sender_Starts_Up()
        {
            await StopSender("Sender1");
            await SendMessages("Sender1", 10);
            await StopSender("Sender1");
            PersistedOutgoingCount().ShouldBe(10);
            StartReceiver("Receiver1");
            StartSender("Sender2");
            await WaitForMessagesToBeProcessed(10);

            PersistedIncomingCount().ShouldBe(0);
            PersistedIncomingCount().ShouldBe(0);

            ReceivedMessageCount().ShouldBe(10);
        }

        [Fact]
        public async Task Sending_Resumes_when_the_Receiver_is_Detected()
        {
            StartSender("Sender1");
            await SendMessages("Sender1", 5);
            StartReceiver("Receiver1");
            await WaitForMessagesToBeProcessed(5);
            PersistedIncomingCount().ShouldBe(0);
            PersistedOutgoingCount().ShouldBe(0);
            ReceivedMessageCount().ShouldBe(5);

        }
    }
}