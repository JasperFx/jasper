using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly SerializationGraph _serializers;
        private readonly IChannelGraph _channels;
        private readonly HandlerGraph _handlers;
        private readonly IMessageLogger _logger;
        private readonly UriAliasLookup _lookup;
        private readonly MessagingSettings _settings;
        private readonly WorkersGraph _workers;

        private ImHashMap<Type, MessageRoute[]> _routes = ImHashMap<Type, MessageRoute[]>.Empty;

        // TODO -- take in MessagingRoot instead?
        public MessageRouter(MessagingSerializationGraph serializers, IChannelGraph channels,
            HandlerGraph handlers, IMessageLogger logger, UriAliasLookup lookup,
            MessagingSettings settings)
        {
            _serializers = serializers;
            _channels = channels;
            _handlers = handlers;
            _logger = logger;
            _lookup = lookup;
            _settings = settings;
            _workers = _settings.Workers;
        }

        public void ClearAll()
        {
            _routes = ImHashMap<Type, MessageRoute[]>.Empty;
        }

        public MessageRoute[] Route(Type messageType)
        {
            if (_routes.TryFind(messageType, out var routes)) return routes;

            routes = compileRoutes(messageType).ToArray();
            _routes = _routes.AddOrUpdate(messageType, routes);

            return routes;
        }

        public MessageRoute RouteForDestination(Envelope envelope)
        {
            envelope.Destination = _lookup.Resolve(envelope.Destination);

            var messageType = envelope.Message.GetType();
            var routes = Route(messageType);

            var candidate = routes.FirstOrDefault(x => x.MatchesEnvelope(envelope));
            if (candidate != null) return candidate;


            var modelWriter = _serializers.WriterFor(messageType);
            var contentType = envelope.ContentType ?? envelope.AcceptedContentTypes.Intersect(modelWriter.ContentTypes).FirstOrDefault()
                              ?? "application/json";


            var channel = _channels.GetOrBuildChannel(envelope.Destination);
            return new MessageRoute(
                       messageType,
                       modelWriter,
                       channel,
                       contentType);
        }

        public Envelope[] Route(Envelope envelope)
        {
            var envelopes = route(envelope);
            foreach (var outgoing in envelopes)
            {
                outgoing.Source = _settings.NodeId;

                // TODO -- watch this
                if (!envelope.RequiresLocalReply)
                {
                    outgoing.ReplyUri = envelope.ReplyUri ?? _channels.SystemReplyUri;
                }

            }

            return envelopes;

        }

        private Envelope[] route(Envelope envelope)
        {
            if (envelope.Destination == null)
            {
                var routes = compileRoutes(envelope.Message.GetType());

                var outgoing = routes.Select(x => x.CloneForSending(envelope)).ToArray();

                // A hack.
                if (outgoing.Length == 1)
                {
                    outgoing[0].Id = envelope.Id;
                }

                return outgoing;
            }

            var route = RouteForDestination(envelope);


            var toBeSent = route.CloneForSending(envelope);
            toBeSent.Id = envelope.Id;

            return new Envelope[]{toBeSent};
        }

        private List<MessageRoute> compileRoutes(Type messageType)
        {
            var list = new List<MessageRoute>();

            var modelWriter = _serializers.WriterFor(messageType);
            var supported = modelWriter.ContentTypes;

            applyLocalPublishingRules(messageType, list);

            applyStaticPublishingRules(messageType, supported, list, modelWriter);

            if (!list.Any())
            {
                if (_handlers.CanHandle(messageType))
                {
                    list.Add(createLocalRoute(messageType));
                }
            }

            return list;
        }

        private void applyStaticPublishingRules(Type messageType, string[] supported, List<MessageRoute> list, ModelWriter modelWriter)
        {
            foreach (var channel in _channels.AllKnownChannels().Where(x => x.ShouldSendMessage(messageType)))
            {
                var contentType = supported.FirstOrDefault(x => x != "application/json") ?? "application/json";

                if (contentType.IsNotEmpty())
                {
                    list.Add(new MessageRoute(messageType, modelWriter, channel, contentType) { });
                }
            }
        }

        private void applyLocalPublishingRules(Type messageType, List<MessageRoute> list)
        {
            if (_handlers.CanHandle(messageType) && _settings.LocalPublishing.Any(x => x.Matches(messageType)))
            {
                var route = createLocalRoute(messageType);

                list.Add(route);
            }
        }

        private MessageRoute createLocalRoute(Type messageType)
        {
            var destination = _workers.LoopbackUriFor(messageType);
            var route = new MessageRoute(messageType, destination, "application/json")
            {
                Channel = _channels.GetOrBuildChannel(destination)
            };
            return route;
        }
    }
}
