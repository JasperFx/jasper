using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using Baseline;

namespace JasperBus.Transports.InMemory
{
    public class InMemoryTransport : ITransport
    {
        private readonly InMemorySettings _settings;
        private Uri _replyUri;
        private readonly ConcurrentDictionary<int, InMemoryQueue> _queues = new ConcurrentDictionary<int, InMemoryQueue>();

        public InMemoryTransport(InMemorySettings settings)
        {
            _settings = settings;
        }

        public string Protocol => "memory";

        public Task Send(Uri uri, byte[] data, IDictionary<string, string> headers)
        {
            if (_queues.Count == 0) throw new InvalidOperationException("There are no available channels with which to send");

            return _queues.Values.First().Send(data, headers, uri, uri.Segments.Last());
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;

            var replyNode = nodes.FirstOrDefault(x => x.Incoming) ??
                            channels.AddChannelIfMissing(_settings.DefaultReplyUri);

            replyNode.Incoming = true;
            _replyUri = replyNode.Uri;


            var groups = nodes.GroupBy(x => x.Uri.Port);

            foreach (var group in groups)
            {
                var queue = _queues.GetOrAdd(group.Key, key => new InMemoryQueue(group.Key, _settings));
                queue.Start(channels, group);

                foreach (var node in group)
                {
                    node.Destination = node.Uri;
                    node.ReplyUri = _replyUri;
                    var subQueue = node.Destination.Segments.Last();
                    node.Sender = new InMemorySender(node.Destination, queue, subQueue);

                    if (node.Incoming)
                    {
                        queue.ListenForMessages(subQueue, new Receiver(pipeline, channels, node));
                    }
                }
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void Dispose()
        {
            _queues.Values.Each(_ => _.Dispose());
        }
    }
}
