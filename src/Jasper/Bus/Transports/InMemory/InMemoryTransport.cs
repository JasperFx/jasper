using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.InMemory
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
            channels.AddChannelIfMissing(Retries);

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

                _queue.ListenForMessages(node, pipeline, channels);

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

        public static readonly Uri Delayed = "memory://delayed".ToUri();
        public static readonly Uri Retries = "memory://retries".ToUri();
    }
}
