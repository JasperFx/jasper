using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime.Routing;


public class MessageTypeRouting
{
    private readonly IList<IMessageRoute> _routes = new List<IMessageRoute>();
    private readonly JasperRuntime _runtime;

    private ImHashMap<Uri, StaticRoute> _destinations = ImHashMap<Uri, StaticRoute>.Empty;

    private ImHashMap<string, IMessageRoute[]> _topicRoutes = ImHashMap<string, IMessageRoute[]>.Empty;

    internal MessageTypeRouting(Type messageType, JasperRuntime runtime)
    {
        MessageType = messageType;
        MessageTypeName = messageType.ToMessageTypeName();
        Customizations = Customizations.AddRange(findMessageTypeCustomizations(messageType));

        LocalQueue = determineLocalSendingAgent(messageType, runtime);

        _runtime = runtime;
    }

    public Type MessageType { get; }

    public IList<Action<Envelope>> Customizations { get; } = new List<Action<Envelope>>();

    public string MessageTypeName { get; }

    public ISendingAgent LocalQueue { get; }

    public IEnumerable<IMessageRoute> Routes => _routes;

    public void AddStaticRoute(ISendingAgent agent)
    {
        var route = new StaticRoute(agent, this);
        _routes.Add(route);
    }

    public void AddTopicRoute(ITopicRule rule, ITopicRouter router)
    {
        var route = new TopicRoute(rule, router, this);
        _routes.Add(route);
    }

    private static ISendingAgent determineLocalSendingAgent(Type messageType, JasperRuntime runtime)
    {
        if (messageType.HasAttribute<LocalQueueAttribute>())
        {
            var queueName = messageType.GetAttribute<LocalQueueAttribute>()!.QueueName;
            return runtime.AgentForLocalQueue(queueName);
        }

        var subscribers = runtime.Subscribers.OfType<LocalQueueSettings>()
            .Where(x => x.ShouldSendMessage(messageType))
            .Select(x => x.Agent)
            .ToArray();

        return subscribers.FirstOrDefault() ?? runtime.GetOrBuildSendingAgent(TransportConstants.LocalUri);
    }

    private static IEnumerable<Action<Envelope>> findMessageTypeCustomizations(Type messageType)
    {
        foreach (var att in messageType.GetAllAttributes<ModifyEnvelopeAttribute>())
            yield return e => att.Modify(e);
    }


    public void RouteToDestination(Envelope envelope)
    {
        if (!_destinations!.TryFind(envelope.Destination, out var route))
        {
            route = DetermineDestinationRoute(envelope.Destination!);
            _destinations = _destinations!.AddOrUpdate(envelope.Destination, route)!;
        }

        route.Configure(envelope);
    }

    public Envelope[] RouteByMessage(object message)
    {
        var envelopes = new Envelope[_routes.Count];
        for (var i = 0; i < _routes.Count; i++)
        {
            envelopes[i] = _routes[i].BuildForSending(message);
        }

        return envelopes;
    }

    public Envelope[] RouteByEnvelope(Type messageType, Envelope envelope)
    {
        if (_routes.Count == 1)
        {
            _routes[0].Configure(envelope);

            return new[] { envelope };
        }

        var envelopes = new Envelope[_routes.Count];
        for (var i = 0; i < _routes.Count; i++)
        {
            envelopes[i] = _routes[i].CloneForSending(envelope);
        }

        return envelopes;
    }

    public StaticRoute DetermineDestinationRoute(Uri destination)
    {
        var agent = _runtime.GetOrBuildSendingAgent(destination);

        return new StaticRoute(agent, this);
    }

    public Envelope[] RouteToTopic(Type messageType, Envelope envelope)
    {
        var topicName = envelope.TopicName;
        if (topicName.IsEmpty())
        {
            throw new ArgumentNullException(nameof(envelope), "There is no topic name for this envelope");
        }

        if (!_topicRoutes.TryFind(topicName, out var routes))
        {
            routes = determineTopicRoutes(messageType, topicName);

            _topicRoutes = _topicRoutes.AddOrUpdate(topicName, routes);
        }

        if (routes.Length != 1)
        {
            return routes.Select(x => x.CloneForSending(envelope)).ToArray();
        }

        routes[0].Configure(envelope);
        return new[] { envelope };
    }

    private IMessageRoute[] determineTopicRoutes(Type messageType, string topicName)
    {
        IMessageRoute[] routes;
        var routers = _runtime.Subscribers.OfType<ITopicRouter>()
            .ToArray();

        var matching = routers.Where(x => x.ShouldSendMessage(messageType)).ToArray();

        if (matching.Any())
        {
            routers = matching;
        }
        else if (!routers.Any())
        {
            throw new InvalidOperationException("There are no topic routers registered for this application");
        }

        // ReSharper disable once CoVariantArrayConversion
        routes = routers.Select(x =>
        {
            var uri = x.BuildUriForTopic(topicName);
            var agent = _runtime.GetOrBuildSendingAgent(uri);
            return new StaticRoute(agent, this);
        }).ToArray();
        return routes;
    }

    public void UseLocalQueueAsRoute()
    {
        var route = new StaticRoute(LocalQueue, this);
        _routes.Add(route);
    }

    public override string ToString()
    {
        return $"MessageTypeRouting for {nameof(MessageType)}: {MessageTypeName}";
    }
}
