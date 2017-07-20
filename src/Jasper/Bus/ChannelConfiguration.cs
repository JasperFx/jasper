using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public class ChannelConfiguration
    {
        private readonly ServiceBusFeature _bus;

        internal ChannelConfiguration(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public ChannelExpression ListenForMessagesFrom(Uri uri)
        {
            var node = _bus.Channels[uri];
            node.Incoming = true;

            return new ChannelExpression(_bus.Channels, node);
        }

        public ChannelExpression ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

        public ChannelExpression this[Uri uri]
        {
            get
            {
                var node = _bus.Channels[uri];
                return new ChannelExpression(_bus.Channels, node);
            }
        }

        public ChannelExpression this[string uriString]
        {
            get
            {
                var node = _bus.Channels[uriString.ToUri()];
                return new ChannelExpression(_bus.Channels, node);
            }
        }

        public ChannelExpression Add(string uriString)
        {
            return this[uriString];
        }

        public ChannelExpression Add(Uri uri)
        {
            return this[uri];
        }
    }
}
