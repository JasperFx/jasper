using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Postgresql.Schema;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StoryTeller;
using StoryTeller.Grammars.Tables;
using StorytellerSpecs.Fixtures.Marten.App;
using StorytellerSpecs.Logging;
using Weasel.Core;

namespace StorytellerSpecs.Fixtures.Marten
{
    public class MartenBackedPersistenceFixture : Fixture
    {
        private StorytellerMessageLogger _messageLogger;

        private LightweightCache<string, IHost> _receivers;
        private DocumentStore _receiverStore;

        private LightweightCache<string, IHost> _senders;
        private DocumentStore _sendingStore;
        private Uri _listener;

        public MartenBackedPersistenceFixture()
        {
            Title = "Marten-Backed Durable Messaging";
        }

        public override void SetUp()
        {
            _listener = new Uri($"tcp://localhost:{PortFinder.FindPort(2555)}");
            _messageLogger =
                new StorytellerMessageLogger(new LoggerFactory(), new NulloMetrics(), new JasperOptions());

            _messageLogger.Start(Context);

            _receiverStore = DocumentStore.For(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "receiver";


                _.Schema.For<TraceDoc>();
            });

            _receiverStore.Advanced.Clean.CompletelyRemoveAll();
            _receiverStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync()
                .GetAwaiter().GetResult();


            _sendingStore = DocumentStore.For(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            var advanced = new AdvancedSettings(null);
            var logger = new NullLogger<PostgresqlEnvelopePersistence>();

            new PostgresqlEnvelopePersistence(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString, SchemaName = "receiver"}, advanced, logger).RebuildStorageAsync().GetAwaiter().GetResult();
            new PostgresqlEnvelopePersistence(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString, SchemaName = "sender"}, advanced, logger).RebuildStorageAsync().GetAwaiter().GetResult();

            _sendingStore.Advanced.Clean.CompletelyRemoveAll();
            _sendingStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync().GetAwaiter().GetResult();

            _receivers = new LightweightCache<string, IHost>(key =>
            {
                var registry = new ReceiverApp(_listener);
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                var logger = new StorytellerAspNetCoreLogger(key);

                // Tell Storyteller about the new logger so that it'll be
                // rendered as part of Storyteller's results
                Context.Reporting.Log(logger);

                // This is bootstrapping a Jasper application through the
                // normal ASP.Net Core IWebHostBuilder
                return Host
                    .CreateDefaultBuilder()
                    .ConfigureLogging(x =>
                    {
                        x.SetMinimumLevel(LogLevel.Debug);

                        // Add the logger to the new Jasper app
                        // being built up
                        x.AddProvider(logger);
                    })
                    .UseJasper(registry)
                    .Start();
            });

            _senders = new LightweightCache<string, IHost>(key =>
            {
                var logger = new StorytellerAspNetCoreLogger(key);

                // Tell Storyteller about the new logger so that it'll be
                // rendered as part of Storyteller's results
                Context.Reporting.Log(logger);

                var registry = new SenderApp(_listener);
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                return Host.CreateDefaultBuilder()
                    .ConfigureLogging(x =>
                    {
                        x.SetMinimumLevel(LogLevel.Debug);

                        // Add the logger to the new Jasper app
                        // being built up
                        x.AddProvider(logger);
                    })
                    .UseJasper(registry)
                    .Start();
            });
        }

        public override void TearDown()
        {
            _receivers.Each(x => x.Dispose());
            _receivers.ClearAll();

            _senders.Each(x => x.Dispose());
            _senders.ClearAll();

            _messageLogger.BuildReports().Each(x => Context.Reporting.Log(x));

            _receiverStore.Dispose();
            _receiverStore = null;
            _sendingStore.Dispose();
            _sendingStore = null;
        }

        [FormatAs("Start receiver node {name}")]
        public void StartReceiver(string name)
        {
            _receivers.FillDefault(name);
        }

        [FormatAs("Start sender node {name}")]
        public void StartSender([Default("Sender1")] string name)
        {
            _senders.FillDefault(name);
        }

        [ExposeAsTable("Send Messages")]
        public Task SendFrom([Header("Sending Node")] [Default("Sender1")]
            string sender, [Header("Message Name")] string name)
        {
            return _senders[sender].Services.GetService<IExecutionContext>().SendAsync(new TraceMessage {Name = name});
        }

        [FormatAs("Send {count} messages from {sender}")]
        public async Task SendMessages([Default("Sender1")] string sender, int count)
        {
            var runtime = _senders[sender];

            for (var i = 0; i < count; i++)
            {
                var msg = new TraceMessage {Name = Guid.NewGuid().ToString()};
                await runtime.Services.GetService<IExecutionContext>().SendAsync(msg);
            }
        }

        [FormatAs("The persisted document count in the receiver should be {count}")]
        public int ReceivedMessageCount()
        {
            using (var session = _receiverStore.LightweightSession())
            {
                return session.Query<TraceDoc>().Count();
            }
        }


        [FormatAs("Wait for {count} messages to be processed by the receivers")]
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

            StoryTellerAssert.Fail("All messages were not received");
        }

        [FormatAs("There should be {count} persisted, incoming messages in the receiver storage")]
        public long PersistedIncomingCount()
        {
            using (var conn = _receiverStore.Tenancy.Default.Database.CreateConnection())
            {
                conn.Open();

                return (long) conn.CreateCommand(
                        $"select count(*) from receiver.{DatabaseConstants.IncomingTable}")
                    .ExecuteScalar();
            }
        }

        [FormatAs("There should be {count} persisted, outgoing messages in the sender storage")]
        public long PersistedOutgoingCount()
        {
            using (var conn = _sendingStore.Tenancy.Default.Database.CreateConnection())
            {
                conn.Open();

                return (long) conn.CreateCommand(
                        $"select count(*) from sender.{DatabaseConstants.OutgoingTable}")
                    .ExecuteScalar();
            }
        }

        [FormatAs("Receiver node {name} stops")]
        public async Task StopReceiver(string name)
        {
            var receiver = _receivers[name];
            await receiver.StopAsync();
            receiver.Dispose();
            _receivers.Remove(name);
        }

        [FormatAs("Sender node {name} stops")]
        public async Task StopSender(string name)
        {
            var sender = _senders[name];
            await sender.StopAsync();
            sender.Dispose();
            _senders.Remove(name);
        }
    }

}
