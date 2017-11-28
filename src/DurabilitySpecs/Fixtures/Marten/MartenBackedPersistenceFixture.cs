using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using DurabilitySpecs.Fixtures.Marten.App;
using Jasper;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Marten.Tests.Setup;
using Marten;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace DurabilitySpecs.Fixtures.Marten
{
    public class MartenBackedPersistenceFixture : Fixture
    {
        private DocumentStore _receiverStore;
        private DocumentStore _sendingStore;

        private readonly LightweightCache<string, JasperRuntime> _receivers
            = new LightweightCache<string, JasperRuntime>(name => JasperRuntime.For<ReceiverApp>());

        private readonly LightweightCache<string, JasperRuntime> _senders
            = new LightweightCache<string, JasperRuntime>(name => JasperRuntime.For<SenderApp>());

        public MartenBackedPersistenceFixture()
        {
            Title = "Marten-Backed Durable Messaging";
        }

        public override void SetUp()
        {
            _receivers.ClearAll();
            _senders.ClearAll();

            _receiverStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            _receiverStore.Advanced.Clean.DeleteAllDocuments();

            _sendingStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            _sendingStore.Advanced.Clean.DeleteAllDocuments();
        }

        public override void TearDown()
        {
            _receivers.Each(x => x.SafeDispose());
            _receivers.ClearAll();

            _senders.Each(x => x.SafeDispose());
            _senders.ClearAll();



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
        public void StartSender([Default("Sender1")]string name)
        {
            _senders.FillDefault(name);
        }

        [ExposeAsTable("Send Messages")]
        public Task SendFrom([Header("Sending Node"), Default("Sender1")]string sender, [Header("Message Name")]string name)
        {
            return _senders[sender].Bus.Send(new TraceMessage {Name = name});
        }

        [FormatAs("Send {count} messages from {sender}")]
        public async Task SendMessages([Default("Sender1")]string sender, int count)
        {
            var runtime = _senders[sender];

            for (int i = 0; i < count; i++)
            {
                var msg = new TraceMessage {Name = Guid.NewGuid().ToString()};
                await runtime.Bus.Send(msg);
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
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    while (stopwatch.ElapsedMilliseconds < 5000)
                    {
                        var actual = await session.Query<TraceDoc>().CountAsync();
                        var envelopeCount = await session.Query<Envelope>()
                            .Where(x => x.Status == TransportConstants.Incoming).CountAsync();

                        if (actual == count && envelopeCount == 0)
                        {
                            return;
                        }

                        await Task.Delay(100);
                    }
                }
                finally
                {
                    stopwatch.Stop();
                }
            }

            StoryTellerAssert.Fail("All messages were not received");
        }

        [FormatAs("There should be {count} persisted, incoming messages in the receiver storage")]
        public int PersistedIncomingCount()
        {
            using (var session = _receiverStore.QuerySession())
            {
                return session.Query<Envelope>().Count(x => x.Status == TransportConstants.Incoming);
            }
        }

        [FormatAs("There should be {count} persisted, outgoing messages in the sender storage")]
        public int PersistedOutgoingCount()
        {
            using (var session = _sendingStore.QuerySession())
            {
                return session.Query<Envelope>().Count(x => x.Status == TransportConstants.Outgoing);
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



    // TODO -- need to have SocketSenderProtocol injected
}
