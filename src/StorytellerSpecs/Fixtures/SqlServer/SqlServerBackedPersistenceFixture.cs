using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.TestSupport.Storyteller.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoryTeller;
using StoryTeller.Grammars.Tables;
using StorytellerSpecs.Fixtures.SqlServer.App;
using Weasel.Core;

namespace StorytellerSpecs.Fixtures.SqlServer
{
    public class SqlServerBackedPersistenceFixture : Fixture
    {
        private StorytellerMessageLogger _messageLogger;

        private LightweightCache<string, IHost> _receivers;

        private LightweightCache<string, IHost> _senders;
        private SenderLatchDetected _senderWatcher;

        public SqlServerBackedPersistenceFixture()
        {
            Title = "SqlServer-Backed Durable Messaging";
        }

        public override void SetUp()
        {
            _messageLogger =
                new StorytellerMessageLogger(new LoggerFactory(), new NulloMetrics(), new JasperOptions());

            _messageLogger.Start(Context);

            _senderWatcher = new SenderLatchDetected(new LoggerFactory());

            new SqlServerEnvelopeStorageAdmin(new SqlServerSettings
                {ConnectionString = Servers.SqlServerConnectionString, SchemaName = "receiver"}).RecreateAll();
            new SqlServerEnvelopeStorageAdmin(new SqlServerSettings
                {ConnectionString = Servers.SqlServerConnectionString, SchemaName = "sender"}).RecreateAll();

            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                conn.CreateCommand(@"
IF OBJECT_ID('receiver.trace_doc', 'U') IS NOT NULL
  drop table receiver.trace_doc;

").ExecuteNonQuery();

                conn.CreateCommand(@"
create table receiver.trace_doc
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQuery();
            }

            _receivers = new LightweightCache<string, IHost>(key =>
            {
                var registry = new ReceiverApp();
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                return JasperHost.For(registry);
            });

            _senders = new LightweightCache<string, IHost>(key =>
            {
                var registry = new SenderApp();
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                registry.Services.For<ITransportLogger>().Use(_senderWatcher);

                return JasperHost.For(registry);
            });
        }

        public override void TearDown()
        {
            _receivers.Each(x => x.Dispose());
            _receivers.ClearAll();

            _senders.Each(x => x.Dispose());
            _senders.ClearAll();

            _messageLogger.BuildReports().Each(x => Context.Reporting.Log(x));
        }

        [FormatAs("Start receiver node {name}")]
        public void StartReceiver(string name)
        {
            _receivers.FillDefault(name);
        }

        [FormatAs("Start sender node {name}")]
        public void StartSender([Default("Sender1")] string name)
        {
            _senderWatcher.Reset();
            _senders.FillDefault(name);
        }

        [ExposeAsTable("Send Messages")]
        public Task SendFrom([Header("Sending Node")] [Default("Sender1")]
            string sender, [Header("Message Name")] string name)
        {
            return _senders[sender].Send(new TraceMessage {Name = name});
        }

        [FormatAs("Send {count} messages from {sender}")]
        public async Task SendMessages([Default("Sender1")] string sender, int count)
        {
            var runtime = _senders[sender];

            for (var i = 0; i < count; i++)
            {
                var msg = new TraceMessage {Name = Guid.NewGuid().ToString()};
                await runtime.Send(msg);
            }
        }

        [FormatAs("The persisted document count in the receiver should be {count}")]
        public int ReceivedMessageCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();
                return (int) conn.CreateCommand("select count(*) from receiver.trace_doc").ExecuteScalar();
            }
        }


        [FormatAs("Wait for {count} messages to be processed by the receivers")]
        public void WaitForMessagesToBeProcessed(int count)
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                for (var i = 0; i < 200; i++)
                {
                    var actual = (int) conn.CreateCommand("select count(*) from receiver.trace_doc").ExecuteScalar();


                    var envelopeCount = PersistedIncomingCount();


                    Trace.WriteLine($"waitForMessages: {actual} actual & {envelopeCount} incoming envelopes");

                    if (actual == count && envelopeCount == 0) return;

                    Thread.Sleep(250);
                }
            }


            StoryTellerAssert.Fail("All messages were not received");
        }

        [FormatAs("There should be {count} persisted, incoming messages in the receiver storage")]
        public int PersistedIncomingCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                return (int) conn.CreateCommand(
                        $"select count(*) from receiver.{DataAccessor.IncomingTable}")
                    .ExecuteScalar();
            }
        }

        [FormatAs("There should be {count} persisted, outgoing messages in the sender storage")]
        public int PersistedOutgoingCount()
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                return (int) conn.CreateCommand(
                        $"select count(*) from sender.{DataAccessor.OutgoingTable}")
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


    public class SenderLatchDetected : TransportLogger
    {
        public TaskCompletionSource<bool> Waiter = new TaskCompletionSource<bool>();

        public SenderLatchDetected(ILoggerFactory factory) : base(factory, new NulloMetrics())
        {
        }

        public Task<bool> Received => Waiter.Task;

        public override void CircuitResumed(Uri destination)
        {
            if (destination == ReceiverApp.Listener) Waiter.TrySetResult(true);

            base.CircuitResumed(destination);
        }

        public override void CircuitBroken(Uri destination)
        {
            if (destination == ReceiverApp.Listener) Reset();

            base.CircuitBroken(destination);
        }

        public void Reset()
        {
            Waiter = new TaskCompletionSource<bool>();
        }
    }
}
