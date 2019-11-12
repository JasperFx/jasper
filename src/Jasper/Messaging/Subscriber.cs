using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging
{
    public class Subscriber : ISubscriber
    {
        private ISendingAgent _agent;
        private IMessageLogger _logger;

        public Subscriber(Uri uri, IEnumerable<Subscription> subscriptions)
        {
            Uri = uri;
            Subscriptions.AddRange(subscriptions);
        }


        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();
        public Uri Uri { get; }


        public bool ShouldSendMessage(Type messageType)
        {
            return Subscriptions.Any(x => x.Matches(messageType));
        }

        public Uri ReplyUri { get; private set; }

        public async Task Send(Envelope envelope)
        {
            envelope.Status = TransportConstants.Outgoing;

            await _agent.StoreAndForward(envelope);

            _logger.Sent(envelope);
        }

        public bool IsDurable => _agent.IsDurable;

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

        public bool SupportsNativeScheduledSend => _agent.SupportsNativeScheduledSend;
    }
}
