using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime.Routing
{
    public interface IMessageTypeRouteCollection
    {
        IEnumerable<IMessageRoute> Routes { get; }
        void AddStaticRoute(ISendingAgent? agent);
        void AddTopicRoute(ITopicRule rule, ITopicRouter router);
    }

    public class MessageTypeRouting : IMessageTypeRouteCollection
    {
        private readonly IList<Action<Envelope>> _customizations = new List<Action<Envelope>>();
        private readonly IJasperRuntime _root;

        private readonly IList<IMessageRoute> _routes = new List<IMessageRoute>();

        public Type MessageType { get; }

        public MessageTypeRouting(Type messageType, IJasperRuntime root)
        {
            MessageType = messageType;
            MessageTypeName = messageType.ToMessageTypeName();
            _customizations = _customizations.AddRange(findMessageTypeCustomizations(messageType));

            LocalQueue = determineLocalSendingAgent(messageType, root);

            _root = root;
        }

        public IEnumerable<IMessageRoute> Routes => _routes;

        public void AddStaticRoute(ISendingAgent? agent)
        {
            var route = new StaticRoute(agent, this);
            _routes.Add(route);
        }

        public void AddTopicRoute(ITopicRule rule, ITopicRouter router)
        {
            var route = new TopicRoute(rule, router, _root, this);
            _routes.Add(route);
        }

        public IList<Action<Envelope>> Customizations => _customizations;

        private static ISendingAgent? determineLocalSendingAgent(Type messageType, IJasperRuntime root)
        {
            if (messageType.HasAttribute<LocalQueueAttribute>())
            {
                var queueName = messageType.GetAttribute<LocalQueueAttribute>().QueueName;
                return root.Runtime.AgentForLocalQueue(queueName);
            }

            var subscribers = root.Runtime.FindLocalSubscribers(messageType);
            return subscribers.FirstOrDefault() ?? root.Runtime.GetOrBuildSendingAgent(TransportConstants.LocalUri);
        }

        public string? MessageTypeName { get; }

        public ISendingAgent? LocalQueue { get; }

        private static IEnumerable<Action<Envelope>> findMessageTypeCustomizations(Type messageType)
        {
            foreach (var att in messageType.GetAllAttributes<ModifyEnvelopeAttribute>())
                yield return e => att.Modify(e);
        }


        private ImHashMap<Uri?, StaticRoute> _destinations = ImHashMap<Uri, StaticRoute>.Empty;


        public void RouteToDestination(Envelope? envelope)
        {
            if (!_destinations.TryFind(envelope.Destination, out var route))
            {
                route = DetermineDestinationRoute(envelope.Destination);
                _destinations = _destinations.AddOrUpdate(envelope.Destination, route);
            }

            route.Configure(envelope);

        }

        public Envelope?[] RouteByMessage(object? message)
        {
            if (_routes.Count == 1)
            {
                return new Envelope?[]{_routes[0].BuildForSending(message)};
            }
            else
            {
                return _routes.Select(x => x.BuildForSending(message)).ToArray();
            }
        }

        public Envelope?[] RouteByEnvelope(Type messageType, Envelope? envelope)
        {
            if (_routes.Count == 1)
            {
                _routes[0].Configure(envelope);

                return new Envelope?[]{envelope};
            }
            else
            {
                return _routes.Select(x => x.CloneForSending(envelope)).ToArray();
            }
        }

        public StaticRoute DetermineDestinationRoute(Uri? destination)
        {
            var agent = _root.Runtime.GetOrBuildSendingAgent(destination);

            return new StaticRoute(agent, this);
        }

        public Envelope?[] RouteToTopic(Type messageType, Envelope? envelope)
        {
            if (envelope.TopicName.IsEmpty()) throw new ArgumentNullException(nameof(envelope), "There is no topic name for this envelope");

            if (!_topicRoutes.TryFind(envelope.TopicName, out var routes))
            {
                var routers = _root
                    .Runtime
                    .FindTopicRoutersForMessageType(messageType);

                if (!routers.Any())
                {
                    throw new InvalidOperationException("There are no topic routers registered for this application");
                }

                routes = routers.Select(x =>
                {
                    var uri = x.BuildUriForTopic(envelope.TopicName);
                    var agent = _root.Runtime.GetOrBuildSendingAgent(uri);
                    return new StaticRoute(agent, this);
                }).ToArray();

                _topicRoutes = _topicRoutes.AddOrUpdate(envelope.TopicName, routes);
            }

            if (routes.Length == 1)
            {
                routes[0].Configure(envelope);
                return new Envelope?[]{envelope};
            }
            else
            {
                return routes.Select(x => x.CloneForSending(envelope)).ToArray();
            }
        }

        private ImHashMap<string?, IMessageRoute[]> _topicRoutes = ImHashMap<string, IMessageRoute[]>.Empty;

        public void UseLocalQueueAsRoute()
        {
            var route = new StaticRoute(LocalQueue, this);
            _routes.Add(route);
        }
    }
}
