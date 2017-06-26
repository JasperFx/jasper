using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.LightningQueues;
using Jasper.Bus.Transports.Lightweight;

namespace Jasper.LightningDb.Transport
{
    public class PersistentTransport : ITransport
    {
        private readonly LightningDbSettings _settings;
        private readonly IBusLogger _logger;
        private readonly SendingAgent _sender;
        private Uri _replyUri;
        private readonly Dictionary<int, PersistentPortListener> _listeners 
            = new Dictionary<int, PersistentPortListener>();

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public static void DeleteAllStorage(string queuePath = null)
        {
            var fileSystem = new FileSystem();
            if (queuePath == null)
            {
                //Find all queues matching queuePath regardless of port.
                var jasperQueuePath = new LightningQueueSettings().QueuePath;
                queuePath = fileSystem.GetDirectory(jasperQueuePath);

                var queues = fileSystem
                    .ChildDirectoriesFor(queuePath)
                    .Where(x => x.StartsWith(jasperQueuePath, StringComparison.OrdinalIgnoreCase));

                queues.Each(x => fileSystem.DeleteDirectory(x));
            }
            else
            {
                fileSystem.DeleteDirectory(queuePath);
            }
        }

        public static string MaxAttemptsHeader = "max-delivery-attempts";
        public static string DeliverByHeader = "deliver-by";

        public PersistentTransport(LightningDbSettings settings, IBusLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void Dispose()
        {
            _cancellation.Cancel();

            _sender.Dispose();

            foreach (var listener in _listeners.Values)
            {
                listener.Dispose();
            }
        }

        public string Protocol => "lq.tcp";

        public Task Send(Envelope envelope, Uri destination)
        {
            // TODO -- persist the message first!!!!!!!

            envelope.Destination = destination;
            envelope.ReplyUri = _replyUri;


            var messagePayload = new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow,
                Destination = destination,
                

                // TODO -- this is awful. Let's get this one optimized ASAP
                Queue = envelope.Destination.Segments.Last(),
            };

            //TODO Maybe expose something to modify transport specific payloads?
            messagePayload.TranslateHeaders();

            _sender.Enqueue(messagePayload);

            return Task.CompletedTask;
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;



            chooseReplyNode(channels, nodes);

            startListening(pipeline, channels);

            // MORE HERE
            // Recover any persisted messages
            // Move existing databases from the old scheme to "incoming" or "outgoing"
        }

        private void startListening(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;


            var groups = nodes.Where(x => x.Incoming).GroupBy(x => x.Uri.Port);
            foreach (var @group in groups)
            {
                var listener = new PersistentPortListener(@group.Key, _logger);
                _listeners.Add(@group.Key, listener);

                foreach (var node in @group)
                {
                    var queueName = node.Uri.Segments.Last();
                    listener.AddQueue(queueName, pipeline, channels, node);
                }
            }

            foreach (var listener in _listeners.Values)
            {
                listener.Start();
            }
        }

        private void chooseReplyNode(ChannelGraph channels, ChannelNode[] nodes)
        {
            var replyNode = nodes.FirstOrDefault(x => x.Incoming);
            if (replyNode == null)
            {
                _replyUri = $"lq.tcp://localhost{_settings.DefaultReplyPort}/replies".ToUri();
                channels.AddChannelIfMissing(_replyUri).Incoming = true;
            }
            else
            {
                _replyUri = replyNode.Destination;
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }
    }
}
