using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly SerializationGraph _serializers;
        private readonly IChannelGraph _channels;
        private readonly ISubscriptionsRepository _subscriptions;
        private readonly HandlerGraph _handlers;
        private readonly CompositeLogger _logger;
        private readonly UriAliasLookup _lookup;

        private readonly ConcurrentDictionary<Type, MessageRoute[]> _routes = new ConcurrentDictionary<Type, MessageRoute[]>();

        public MessageRouter(SerializationGraph serializers, IChannelGraph channels, ISubscriptionsRepository subscriptions, HandlerGraph handlers, CompositeLogger logger, UriAliasLookup lookup)
        {
            _serializers = serializers;
            _channels = channels;
            _subscriptions = subscriptions;
            _handlers = handlers;
            _logger = logger;
            _lookup = lookup;
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
            envelope.Destination = _lookup.Resolve(envelope.Destination);

            var messageType = envelope.Message.GetType();
            var routes = await Route(messageType);

            var candidate = routes.FirstOrDefault(x => x.MatchesEnvelope(envelope));
            if (candidate != null) return candidate;


            var modelWriter = _serializers.WriterFor(messageType);
            var contentType = envelope.ContentType ?? envelope.AcceptedContentTypes.Intersect(modelWriter.ContentTypes).FirstOrDefault()
                              ?? "application/json";

            return new MessageRoute(
                       messageType,
                       modelWriter,
                       envelope.Destination,
                       contentType);
        }

        private async Task<List<MessageRoute>> compileRoutes(Type messageType)
        {
            var list = new List<MessageRoute>();

            var modelWriter = _serializers.WriterFor(messageType);
            var supported = modelWriter.ContentTypes;

            foreach (var channel in _channels.Distinct().Where(x => x.ShouldSendMessage(messageType)))
            {
                var contentType = supported.FirstOrDefault(x => x != "application/json") ?? "application/json";

                if (contentType.IsNotEmpty())
                {
                    list.Add(new MessageRoute(messageType, modelWriter, channel.Destination, contentType));
                }
            }

            var subscriptions = await _subscriptions.GetSubscribersFor(messageType);
            if (subscriptions.Any())
            {
                var published = new PublishedMessage(messageType, modelWriter, _channels);


                foreach (var subscription in subscriptions)
                {
                    if (MessageRoute.TryToRoute(published, subscription, out MessageRoute route,
                        out PublisherSubscriberMismatch mismatch))
                    {
                        route.Writer = modelWriter[route.ContentType];
                        list.Add(route);
                    }
                    else
                    {
                        _logger.SubscriptionMismatch(mismatch);
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
