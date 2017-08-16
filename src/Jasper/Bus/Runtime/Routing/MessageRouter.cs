using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly SerializationGraph _serializers;
        private readonly ChannelGraph _channels;
        private readonly ISubscriptionsRepository _subscriptions;
        private readonly HandlerGraph _handlers;

        private readonly ConcurrentDictionary<Type, MessageRoute[]> _routes = new ConcurrentDictionary<Type, MessageRoute[]>();

        public MessageRouter(SerializationGraph serializers, ChannelGraph channels, ISubscriptionsRepository subscriptions, HandlerGraph handlers)
        {
            _serializers = serializers;
            _channels = channels;
            _subscriptions = subscriptions;
            _handlers = handlers;
        }

        public void ClearAll()
        {
            _routes.Clear();
        }

        public async Task<MessageRoute[]> Route(Type messageType)
        {
            if (!_routes.ContainsKey(messageType))
            {
                var routes = (await compileRoutes(messageType)).ToArray();
                _routes[messageType] = routes;

                return routes;
            }

            return _routes[messageType];
        }

        public async Task<MessageRoute> RouteForDestination(Envelope envelope)
        {

            var messageType = envelope.Message.GetType();
            var routes = await Route(messageType);

            var candidate = routes.FirstOrDefault(x => x.MatchesEnvelope(envelope));
            if (candidate != null) return candidate;


            var modelWriter = _serializers.WriterFor(messageType);
            var contentType = envelope.AcceptedContentTypes.Intersect(modelWriter.ContentTypes).FirstOrDefault()
                              ?? "application/json";

            // TODO -- memoize this some day vs type & destination
            return new MessageRoute(
                       messageType,
                       modelWriter,
                       envelope.Destination,
                       contentType);
        }

        private async Task<List<MessageRoute>> compileRoutes(Type messageType)
        {
            var list = new List<MessageRoute>();

            // TODO -- trace subscriptions that cannot be filled?
            var modelWriter = _serializers.WriterFor(messageType);
            var supported = modelWriter.ContentTypes;

            foreach (var channel in _channels.Distinct().Where(x => x.ShouldSendMessage(messageType)))
            {
                var contentType = channel.AcceptedContentTypes.Intersect(supported).FirstOrDefault();

                if (contentType.IsNotEmpty())
                {
                    list.Add(new MessageRoute(messageType, modelWriter, channel.Destination, contentType));
                }
            }

            foreach (var subscription in await _subscriptions.GetSubscribersFor(messageType))
            {
                if (subscription.Accepts.IsEmpty())
                {
                    list.Add(new MessageRoute(messageType, modelWriter, subscription.Receiver, "application/json"));
                }
                else
                {
                    var accepted = subscription.Accepts.Split(',');

                    var contentType = accepted.Intersect(supported).FirstOrDefault();

                    if (contentType.IsNotEmpty())
                    {
                        list.Add(new MessageRoute(messageType, modelWriter, subscription.Receiver, contentType));
                    }
                }
            }

            if (!list.Any())
            {
                if (_handlers.HandlerFor(messageType) != null && _channels.DefaultChannel != null)
                {
                    list.Add(new MessageRoute(messageType, modelWriter, _channels.DefaultChannel.Uri, "application/json"));
                }
            }

            return list;
        }
    }
}
