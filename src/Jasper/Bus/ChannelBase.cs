using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus
{
    public class Channel : ChannelBase
    {
        private readonly ITransport _transport;

        public Channel(SubscriberAddress address, ITransport transport) : base(address, transport.DefaultReplyUri())
        {
            _transport = transport;
        }

        protected override Task send(Envelope envelope)
        {
            return _transport.Send(envelope, envelope.Destination);
        }
    }

    public abstract class ChannelBase : IChannel
    {
        private readonly SubscriberAddress _address;

        protected ChannelBase(SubscriberAddress address, Uri replyUri)
        {
            ReplyUri = replyUri;
            _address = address;

            Destination = _address.Uri;
        }

        public Uri Uri => _address.Uri;
        public Uri ReplyUri { get; }

        public Task Send(Envelope envelope)
        {
            foreach (var modifier in _address.Modifiers)
            {
                modifier.Modify(envelope);
            }

            return send(envelope);
        }


        protected abstract Task send(Envelope envelope);

        public Uri Destination { get; }
        public Uri Alias => _address.Alias;

        public string QueueName()
        {
            return _address.Uri.QueueName();
        }

        public bool ShouldSendMessage(Type messageType)
        {
            return _address.ShouldSendMessage(messageType);
        }
    }
}
