using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.LightningQueues
{
    public class LightningQueuesTransport : ITransport
    {
        public static void DeleteAllStorage(string queuePath = null)
        {
            var fileSystem = new FileSystem();
            if (queuePath == null)
            {
                //Find all queues matching queuePath regardless of port.
                var jasperQueuePath = new LightningQueueSettings().QueuePath;
                queuePath = fileSystem.GetDirectory(jasperQueuePath);

                var queues = fileSystem.ChildDirectoriesFor(queuePath)
                    .Where(x => x.StartsWith(jasperQueuePath, StringComparison.OrdinalIgnoreCase));

                queues.Each(x => fileSystem.DeleteDirectory(x));
            }
            else
            {
                fileSystem.DeleteDirectory(queuePath);
            }
        }


        private readonly ConcurrentDictionary<int, LightningQueue> _queues = new ConcurrentDictionary<int, LightningQueue>();
        private readonly LightningQueueSettings _settings;
        private readonly ConcurrentDictionary<Uri, LightningUri> _uris = new ConcurrentDictionary<Uri, LightningUri>();
        private readonly ITransportLogger[] _loggers;
        private Uri _replyUri;

        public LightningQueuesTransport(LightningQueueSettings settings, ITransportLogger[] loggers)
        {
            _settings = settings;
            _loggers = loggers;
        }

        public void Dispose()
        {
            _queues.Values.Each(x => x.Dispose());
            _queues.Clear();
        }

        public string Protocol => "lq.tcp";

        public Uri ReplyUriFor(Uri address)
        {
            return _replyUri;
        }

        private LightningUri lqUriFor(Uri uri)
        {
            return _uris.GetOrAdd(uri, u => new LightningUri(u));
        }

        public Task Send(Envelope envelope, Uri destination)
        {
            if (_queues.Count == 0) throw new InvalidOperationException("There are no available LightningQueues channels with which to send");

            var lqUri = lqUriFor(destination);
            _queues.Values.First().Send(envelope, lqUri.Address, lqUri.QueueName);

            return Task.CompletedTask;
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;

            var replyNode = nodes.FirstOrDefault(x => x.Incoming) ??
                            channels.AddChannelIfMissing(_settings.DefaultReplyUri);

            replyNode.Incoming = true;
            _replyUri = replyNode.Uri.ToLightningUri().Address;


            var groups = nodes.GroupBy(x => x.Uri.Port);

            foreach (var group in groups)
            {
                // TODO -- need to worry about persistence or not here
                var queue = _queues.GetOrAdd(group.Key, key => new LightningQueue(group.Key, true, _settings, _loggers));
                queue.Start(channels, group);

                foreach (var node in group)
                {
                    var lightningUri = node.Uri.ToLightningUri();
                    node.Destination = lightningUri.Address;
                    node.ReplyUri = _replyUri;
                    node.Sender = new QueueSender(node.Destination, queue, node.Destination.ToLightningUri().QueueName);

                    if (node.Incoming)
                    {
                        queue.ListenForMessages(lightningUri.QueueName, new Receiver(pipeline, channels, node));
                    }
                }
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }
    }
}
