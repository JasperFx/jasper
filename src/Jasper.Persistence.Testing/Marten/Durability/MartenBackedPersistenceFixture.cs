using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.Persistence.Testing.Marten.Durability.App;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Durability
{
    [Collection("marten")]
    public class MartenBackedPersistenceFixture : IAsyncLifetime
    {
        private LightweightCache<string, IHost> _receivers;
        private DocumentStore _receiverStore;

        private LightweightCache<string, IHost> _senders;
        private SenderLatchDetected _senderWatcher;
        private DocumentStore _sendingStore;

        public async Task InitializeAsync()
        {
            _senderWatcher = new SenderLatchDetected(new LoggerFactory());

            _receiverStore = DocumentStore.For(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "receiver";


                _.Schema.For<TraceDoc>();
            });

            await _receiverStore.Advanced.Clean.CompletelyRemoveAllAsync();
            await _receiverStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync();


            _sendingStore = DocumentStore.For(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            await new PostgresqlEnvelopeStorageAdmin(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString, SchemaName = "receiver"}).RebuildSchemaObjects();


            await new PostgresqlEnvelopeStorageAdmin(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString, SchemaName = "sender"}).RebuildSchemaObjects();

            await _sendingStore.Advanced.Clean.CompletelyRemoveAllAsync();
            _sendingStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync().GetAwaiter().GetResult();

            _receivers = new LightweightCache<string, IHost>(key =>
            {
                var registry = new ReceiverApp();

                // This is bootstrapping a Jasper application through the
                // normal ASP.Net Core IWebHostBuilder
                return Host
                    .CreateDefaultBuilder()
                    .ConfigureLogging(x =>
                    {
                        x.SetMinimumLevel(LogLevel.Debug);
                    })
                    .UseJasper(registry)
                    .Start();
            });

            _senders = new LightweightCache<string, IHost>(key =>
            {
                var registry = new SenderApp();

                registry.Services.For<ITransportLogger>().Use(_senderWatcher);


                return Host.CreateDefaultBuilder()
                    .ConfigureLogging(x =>
                    {
                        x.SetMinimumLevel(LogLevel.Debug);

                    })
                    .UseJasper(registry)
                    .Start();
            });
        }

        public Task DisposeAsync()
        {
            _receivers.Each(x => x.Dispose());
            _receivers.ClearAll();

            _senders.Each(x => x.Dispose());
            _senders.ClearAll();

            _receiverStore.Dispose();
            _receiverStore = null;
            _sendingStore.Dispose();
            _sendingStore = null;

            return Task.CompletedTask;

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
                await runtime.Services.GetService<IExecutionContext>().Send(msg);
            }
        }

        public int ReceivedMessageCount()
        {
            using (var session = _receiverStore.LightweightSession())
            {
                return session.Query<TraceDoc>().Count();
            }
        }


        public async Task WaitForMessagesToBeProcessed(int count)
        {
            using (var session = _receiverStore.QuerySession())
            {
                for (var i = 0; i < 200; i++)
                {
                    var actual = session.Query<TraceDoc>().Count();
                    var envelopeCount = PersistedIncomingCount();


                    if (actual == count && envelopeCount == 0) return;

                    await Task.Delay(250);
                }
            }

            throw new Exception("All messages were not received");
        }

        public long PersistedIncomingCount()
        {
            using (var conn = _receiverStore.Tenancy.Default.CreateConnection())
            {
                conn.Open();

                return (long) conn.CreateCommand(
                        $"select count(*) from receiver.{DatabaseConstants.IncomingTable}")
                    .ExecuteScalar();
            }
        }

        public long PersistedOutgoingCount()
        {
            using (var conn = _sendingStore.Tenancy.Default.CreateConnection())
            {
                conn.Open();

                return (long) conn.CreateCommand(
                        $"select count(*) from sender.{DatabaseConstants.OutgoingTable}")
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