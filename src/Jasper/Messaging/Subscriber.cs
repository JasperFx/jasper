using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging
{
    public class Subscriber : ISubscriber
    {
        private IMessageLogger _logger;
        private ISendingAgent _agent;
        public Uri Uri { get; private set; }

        public Uri Alias { get; private set; }

        public Subscriber(Uri uri)
        {
            Uri = uri;
        }


        public IList<RoutingRule> Rules { get; } = new List<RoutingRule>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Rules.Any(x => x.Matches(messageType));
        }

        public void ReadAlias(UriAliasLookup lookups)
        {
            var real = lookups.Resolve(Uri);
            if (real != Uri)
            {
                Alias = Uri;
                Uri = real;
            }
        }

        public override string ToString()
        {
            return $"Subscriber: {Uri}";
        }

        public void StartSending(IMessageLogger logger, ISendingAgent agent, Uri replyUri)
        {
            ReplyUri = replyUri;
            _logger = logger;
            _agent = agent;

        }

        public Uri ReplyUri { get; private set; }

        public async Task Send(Envelope envelope)
        {
            envelope.Status = TransportConstants.Outgoing;

            await _agent.StoreAndForward(envelope);

            _logger.Sent(envelope);
        }

        public bool IsDurable => _agent.IsDurable;
        public int QueuedCount => _agent.QueuedCount;

        public async Task QuickSend(Envelope envelope)
        {
            envelope.Status = TransportConstants.Outgoing;
            await _agent.EnqueueOutgoing(envelope);
            _logger.Sent(envelope);
        }

        public bool Latched => _agent.Latched;

        public void Dispose()
        {
            _agent?.Dispose();
        }
    }
}
