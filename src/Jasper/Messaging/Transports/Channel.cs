using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    // For *now*, saying that there'll always be a static channel for every
    // outgoing destination. Not a huge problem I believe
    public class Channel : IChannel
    {
        private readonly IMessageLogger _logger;
        private readonly SubscriberAddress _address;
        private readonly ISendingAgent _agent;
        public Uri Uri => _address.Uri;
        public Uri LocalReplyUri { get; }

        public Channel(IMessageLogger logger, SubscriberAddress address, Uri replyUri, ISendingAgent agent)
        {
            LocalReplyUri = replyUri;
            _logger = logger;
            _address = address;
            _agent = agent;


        }

        public Channel(IMessageLogger logger, ISendingAgent agent, Uri replyUri)
            : this(logger, new SubscriberAddress(agent.Destination), replyUri, agent)
        {

        }

        public bool ShouldSendMessage(Type messageType)
        {
            return _address.ShouldSendMessage(messageType);
        }

        public async Task Send(Envelope envelope)
        {
            if (envelope.RequiresLocalReply && LocalReplyUri == null)
            {
                throw new InvalidOperationException($"There is no known local reply Uri for channel {_address}, but one is required for this operation");
            }

            ApplyModifications(envelope);

            envelope.Status = TransportConstants.Outgoing;

            await _agent.StoreAndForward(envelope);

            _logger.Sent(envelope);
        }

        public void ApplyModifications(Envelope envelope)
        {
            foreach (var modifier in _address.Modifiers)
            {
                modifier.Modify(envelope);
            }
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
