using System;
using System.Collections.Concurrent;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRoute
    {
        public MessageRoute(Type messageType, Uri destination, string contentType)
        {
            MessageType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public Type MessageType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }
    }

    public interface IMessageRouter
    {
        void ClearAll();
        MessageRoute[] Route(Type messageType);
    }

    public class MessageRouter : IMessageRouter
    {
        private readonly SerializationGraph _serializers;
        private readonly ChannelGraph _channels;
        private readonly ISubscriptionsStorage _subscriptions;

        private readonly ConcurrentDictionary<Type, MessageRoute[]> _routes = new ConcurrentDictionary<Type, MessageRoute[]>();

        public MessageRouter(SerializationGraph serializers, ChannelGraph channels, ISubscriptionsStorage subscriptions)
        {
            _serializers = serializers;
            _channels = channels;
            _subscriptions = subscriptions;
        }

        public void ClearAll()
        {
            _routes.Clear();
        }

        public MessageRoute[] Route(Type messageType)
        {
            // Use subscriptions too
            // Match by writers & what we know about routing rules
            throw new NotImplementedException();
        }
    }
}
