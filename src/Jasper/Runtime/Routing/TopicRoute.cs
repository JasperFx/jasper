using System;
using Baseline.ImTools;
using Jasper.Serialization;
using Jasper.Util;

namespace Jasper.Runtime.Routing
{
    public class TopicRoute : IMessageRoute
    {
        private readonly ITopicRule _rule;
        private readonly ITopicRouter _router;
        private readonly IMessagingRoot _root;
        private readonly MessageTypeRouting _messageTypeRouting;

        public TopicRoute(ITopicRule rule, ITopicRouter router,
            IMessagingRoot root, MessageTypeRouting messageTypeRouting)
        {
            _rule = rule;
            _router = router;
            _root = root;
            _messageTypeRouting = messageTypeRouting;
        }

        public void Configure(Envelope? envelope)
        {
            routeFor(envelope.Message).Configure(envelope);
        }

        public Envelope? CloneForSending(Envelope? envelope)
        {
            return routeFor(envelope.Message).CloneForSending(envelope);
        }

        private IMessageRoute routeFor(object? message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var topicName = _rule.DetermineTopicName(message);
            if (!_routeForTopics.TryFind(topicName, out var route))
            {
                var uri = _router.BuildUriForTopic(topicName);
                route = _messageTypeRouting.DetermineDestinationRoute(uri);
                _routeForTopics = _routeForTopics.AddOrUpdate(topicName, route);
            }

            return route;
        }

        public Envelope? BuildForSending(object? message)
        {
            return routeFor(message).BuildForSending(message);
        }

        private ImHashMap<string?, IMessageRoute> _routeForTopics = ImHashMap<string, IMessageRoute>.Empty;

        public Uri? Destination { get; }
        public string? ContentType { get; }
    }
}
