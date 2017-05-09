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
        private readonly InMemoryQueue _queue;

        public InMemoryTransport(InMemorySettings settings)
        {
            _settings = settings;
            _queue = new InMemoryQueue(settings);
        }

        public string Protocol => "memory";

        public Task Send(Envelope envelope, Uri destination)
        {
            return _queue.Send(envelope, destination);
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;

            var replyNode = nodes.FirstOrDefault(x => x.Incoming) ??
                            channels.AddChannelIfMissing(_settings.DefaultReplyUri);

            replyNode.Incoming = true;
            _replyUri = replyNode.Uri;
            _queue.Start(nodes);

            foreach (var node in nodes)
            {
                node.Destination = node.Uri;
                node.ReplyUri = _replyUri;
                node.Sender = new InMemorySender(node.Uri, _queue);

                if (node.Incoming)
                {
                    _queue.ListenForMessages(node.Uri, new Receiver(pipeline, channels, node));
                }
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}
