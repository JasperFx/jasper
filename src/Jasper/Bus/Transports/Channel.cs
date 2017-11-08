using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Util;

namespace Jasper.Bus.Transports
{
    // For *now*, saying that there'll always be a static channel for every outgoing destination. Not a huge problem I believe
    public class Channel : IChannel
    {
        private readonly SubscriberAddress _address;
        private readonly ISendingAgent _agent;
        public Uri Uri => _address.Uri;
        public Uri LocalReplyUri { get; }

        public Channel(SubscriberAddress address, Uri replyUri, ISendingAgent agent)
        {
            LocalReplyUri = replyUri;
            _address = address;
            _agent = agent;
        }

        public Channel(ISendingAgent agent, Uri replyUri)
            : this(new SubscriberAddress(agent.Destination), replyUri, agent)
        {

        }

        public bool ShouldSendMessage(Type messageType)
        {
            return _address.ShouldSendMessage(messageType);
        }

        // TODO -- will need another mechanism here to just enqueue outgoing

        public Task Send(Envelope envelope)
        {
            foreach (var modifier in _address.Modifiers)
            {
                modifier.Modify(envelope);
            }

            return _agent.StoreAndForward(envelope);
        }

        public void Dispose()
        {
            _agent?.Dispose();
        }
    }
}
