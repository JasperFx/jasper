using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly HandlerGraph _handlers;
        private readonly IMessageLogger _logger;
        private readonly MessagingSerializationGraph _serializers;
        private readonly JasperOptions _settings;
        private readonly ISubscriberGraph _subscribers;
        private readonly WorkersGraph _workers;

        private ImHashMap<Type, MessageRoute[]> _routes = ImHashMap<Type, MessageRoute[]>.Empty;

        public MessageRouter(IMessagingRoot root, HandlerGraph handlers)
        {
            _serializers = root.Serialization;
            _subscribers = root.Subscribers;
            _handlers = handlers;
            _logger = root.Logger;
            _settings = root.Options;
            _workers = _handlers.Workers;
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
            var channel = _subscribers.GetOrBuild(envelope.Destination);


            if (envelope.Message != null)
            {
                var messageType = envelope.Message.GetType();
                var routes = Route(messageType);

                var candidate = routes.FirstOrDefault(x => x.MatchesEnvelope(envelope));
                if (candidate != null) return candidate;


                var modelWriter = _serializers.WriterFor(messageType);
                var contentType = envelope.ContentType ??
                                  envelope.AcceptedContentTypes.Intersect(modelWriter.ContentTypes).FirstOrDefault()
                                  ?? "application/json";

                return new MessageRoute(
                    messageType,
                    modelWriter,
                    channel,
                    contentType);
            }
            else if (envelope.Data != null)
            {
                return new MessageRoute(envelope, channel);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Missing message");
            }




        }

        public Envelope[] Route(Envelope envelope)
        {
            var envelopes = route(envelope);
            foreach (var outgoing in envelopes) outgoing.Source = _settings.NodeId;

            return envelopes;
        }

        private Envelope[] route(Envelope envelope)
        {
            if (envelope.Destination == null)
            {
                var routes = compileRoutes(envelope.Message.GetType());

                var outgoing = routes.Select(x => x.CloneForSending(envelope)).ToArray();

                // A hack.
                if (outgoing.Length == 1) outgoing[0].Id = envelope.Id;

                return outgoing;
            }

            var route = RouteForDestination(envelope);


            var toBeSent = route.CloneForSending(envelope);
            toBeSent.Id = envelope.Id;

            return new[] {toBeSent};
        }

        private List<MessageRoute> compileRoutes(Type messageType)
        {
            var list = new List<MessageRoute>();

            var modelWriter = _serializers.WriterFor(messageType);
            var supported = modelWriter.ContentTypes;

            applyLocalPublishingRules(messageType, list);

            applyStaticPublishingRules(messageType, supported, list, modelWriter);

            if (!list.Any())
                if (_handlers.CanHandle(messageType))
                    list.Add(createLocalRoute(messageType));

            return list;
        }

        private void applyStaticPublishingRules(Type messageType, string[] supported, List<MessageRoute> list,
            WriterCollection<IMessageSerializer> writerCollection)
        {
            foreach (var channel in _subscribers.AllKnown().Where(x => x.ShouldSendMessage(messageType)))
            {
                var contentType = supported.FirstOrDefault(x => x != "application/json") ?? "application/json";

                if (contentType.IsNotEmpty())
                    list.Add(new MessageRoute(messageType, writerCollection, channel, contentType));
            }
        }

        private void applyLocalPublishingRules(Type messageType, List<MessageRoute> list)
        {
            if (_settings.DisableLocalPublishing) return;

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
                Subscriber = _subscribers.GetOrBuild(destination)
            };
            return route;
        }
    }
}
