using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Runtime.Routing
{
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
            return _routes.GetOrAdd(messageType, type => compileRoutes(type).ToArray());
        }

        private IEnumerable<MessageRoute> compileRoutes(Type messageType)
        {
            // TODO -- trace subscriptions that cannot be filled?
            var supported = _serializers.WriterFor(messageType).ContentTypes;

            foreach (var channel in _channels.Distinct().Where(x => x.ShouldSendMessage(messageType)))
            {

                var contentType = channel.AcceptedContentTypes.Intersect(supported).FirstOrDefault();

                if (contentType.IsNotEmpty())
                {
                    yield return new MessageRoute(messageType, channel.Destination, contentType);
                }
            }

            foreach (var subscription in _subscriptions.GetSubscribersFor(messageType))
            {
                if (subscription.Accepts.IsEmpty())
                {
                    yield return new MessageRoute(messageType, subscription.Receiver, "application/json");
                }
                else
                {
                    var accepted = subscription.Accepts.Split(',');

                    var contentType = accepted.Intersect(supported).FirstOrDefault();

                    if (contentType.IsNotEmpty())
                    {
                        yield return new MessageRoute(messageType, subscription.Receiver, contentType);
                    }
                }
            }


        }
    }
}
