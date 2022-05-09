using System;
using Baseline.ImTools;
using Jasper.Util;

namespace Jasper.Runtime.Routing;

public class TopicRoute : IMessageRoute
{
    private readonly MessageTypeRouting _messageTypeRouting;
    private readonly ITopicRouter _router;
    private readonly ITopicRule _rule;

    private ImHashMap<string, IMessageRoute> _routeForTopics = ImHashMap<string, IMessageRoute>.Empty;

    public TopicRoute(ITopicRule rule, ITopicRouter router,
        MessageTypeRouting messageTypeRouting)
    {
        _rule = rule;
        _router = router;
        _messageTypeRouting = messageTypeRouting;
    }

    public void Configure(Envelope envelope)
    {
        routeFor(envelope.Message).Configure(envelope);
    }

    public Envelope CloneForSending(Envelope envelope)
    {
        return routeFor(envelope.Message).CloneForSending(envelope);
    }

    public Envelope BuildForSending(object message)
    {
        return routeFor(message).BuildForSending(message);
    }

    public Uri Destination { get; } = "topics://".ToUri();
    public string ContentType => EnvelopeConstants.JsonContentType;

    private IMessageRoute routeFor(object? message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var topicName = _rule.DetermineTopicName(message);
        if (!_routeForTopics.TryFind(topicName, out var route))
        {
            var uri = _router.BuildUriForTopic(topicName);
            route = _messageTypeRouting.DetermineDestinationRoute(uri);
            _routeForTopics = _routeForTopics.AddOrUpdate(topicName, route);
        }

        return route;
    }
}
