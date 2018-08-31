using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Storyteller.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StorytellerSpecs.Fixtures.SqlServer.App;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.SqlServer
{
    public class SqlServerBackedPersistenceFixture : Fixture
    {
        private StorytellerMessageLogger _messageLogger;

        private LightweightCache<string, JasperRuntime> _receivers;

        private LightweightCache<string, JasperRuntime> _senders;
        private SenderLatchDetected _senderWatcher;

        public SqlServerBackedPersistenceFixture()
        {
            Title = "SqlServer-Backed Durable Messaging";
        }

        public override void SetUp()
        {
            _messageLogger =
                new StorytellerMessageLogger(new MessageHistory(), new LoggerFactory(), new NulloMetrics());

            _messageLogger.Start(Context);

            _senderWatcher = new SenderLatchDetected(new LoggerFactory());

            new SchemaLoader(Servers.SqlServerConnectionString, "receiver").RecreateAll();
            new SchemaLoader(Servers.SqlServerConnectionString, "sender").RecreateAll();

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

            _receivers = new LightweightCache<string, JasperRuntime>(key =>
            {
                var registry = new ReceiverApp();
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                return JasperRuntime.For(registry);
            });

            _senders = new LightweightCache<string, JasperRuntime>(key =>
            {
                var registry = new SenderApp();
                registry.Services.AddSingleton<IMessageLogger>(_messageLogger);

                registry.Services.For<ITransportLogger>().Use(_senderWatcher);

                return JasperRuntime.For(registry);
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
            return _senders[sender].Messaging.Send(new TraceMessage {Name = name});
        }

        [FormatAs("Send {count} messages from {sender}")]
        public async Task SendMessages([Default("Sender1")] string sender, int count)
        {
            var runtime = _senders[sender];

            for (var i = 0; i < count; i++)
            {
                var msg = new TraceMessage {Name = Guid.NewGuid().ToString()};
                await runtime.Messaging.Send(msg);
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
                        $"select count(*) from receiver.{SqlServerEnvelopePersistor.IncomingTable}")
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
                        $"select count(*) from sender.{SqlServerEnvelopePersistor.OutgoingTable}")
                    .ExecuteScalar();
            }
        }

        [FormatAs("Receiver node {name} stops")]
        public void StopReceiver(string name)
        {
            _receivers[name].Dispose();
            _receivers.Remove(name);
        }

        [FormatAs("Sender node {name} stops")]
        public void StopSender(string name)
        {
            _senders[name].Dispose();
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
