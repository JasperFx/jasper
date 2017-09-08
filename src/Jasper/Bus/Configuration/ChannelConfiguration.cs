using System;
using Jasper.Bus.Runtime;
using Jasper.Util;

namespace Jasper.Bus.Configuration
{
    public class ChannelConfiguration
    {
        private readonly ServiceBusFeature _bus;

        internal ChannelConfiguration(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public IQueueSettings ListenForMessagesFrom(Uri uri)
        {
            return _bus.Settings.ListenTo(uri);
        }

        public IQueueSettings ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

        public void DefaultIs(string uriString)
        {
            DefaultIs(uriString.ToUri());
        }

        public void DefaultIs(Uri uri)
        {
            _bus.Settings.DefaultChannelAddress = uri;
        }
    }
}
