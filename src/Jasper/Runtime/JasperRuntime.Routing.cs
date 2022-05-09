using System;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Transports.Sending;

namespace Jasper.Runtime;

public partial class JasperRuntime : IEnvelopeRouter
{
    private ISendingAgent? _durableLocalQueue;
    private ImHashMap<Type, MessageTypeRouting> _routes = ImHashMap<Type, MessageTypeRouting>.Empty;

    public Envelope[] RouteOutgoingByMessage(object message)
    {
        var envelopes = routingFor(message.GetType()).RouteByMessage(message);
        adjustForScheduledSend(envelopes);

        return envelopes;
    }

    public Envelope[] RouteOutgoingByEnvelope(Envelope original)
    {
        var messageType = DetermineMessageType(original);

        var messageTypeRouting = routingFor(messageType);

        var envelopes = original.TopicName.IsEmpty()
            ? messageTypeRouting.RouteByEnvelope(messageType, original)
            : messageTypeRouting.RouteToTopic(messageType, original);

        adjustForScheduledSend(envelopes);

        return envelopes;
    }


    public void RouteToDestination(Uri destination, Envelope envelope)
    {
        envelope.Destination = destination;
        routingFor(envelope).RouteToDestination(envelope);
    }

    public Envelope[] RouteToTopic(string topicName, Envelope envelope)
    {
        var messageTypeRouting = routingFor(envelope);
        envelope.TopicName = topicName;
        var envelopes = messageTypeRouting.RouteToTopic(messageTypeRouting.MessageType, envelope);

        adjustForScheduledSend(envelopes);

        return envelopes;
    }

    Envelope IEnvelopeRouter.RouteLocally<T>(T message) where T : default
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var agent = routingFor(typeof(T)).LocalQueue;

        return new Envelope(message)
        {
            Destination = agent.Destination,
            ContentType = EnvelopeConstants.JsonContentType,
            Sender = agent,
            Serializer = agent.Endpoint.DefaultSerializer
        };
    }

    public Envelope RouteLocally<T>(T message, string workerQueue)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (workerQueue == null)
        {
            throw new ArgumentNullException(nameof(workerQueue));
        }

        var agent = AgentForLocalQueue(workerQueue);

        return new Envelope(message)
        {
            Destination = agent.Destination,
            ContentType = EnvelopeConstants.JsonContentType,
            Sender = agent,
            Serializer = agent.Endpoint.DefaultSerializer
        };
    }

    public MessageTypeRouting RouteByType(Type messageType)
    {
        RegisterMessageType(messageType);
        var routing = new MessageTypeRouting(messageType, this);

        var subscribers = Subscribers
            .Where(x => x.ShouldSendMessage(messageType))
            .ToArray();

        if (subscribers.Any())
        {
            foreach (var subscriber in subscribers) subscriber.AddRoute(routing, this);
        }
        else if (Options.HandlerGraph.CanHandle(messageType))
        {
            routing.UseLocalQueueAsRoute();
        }

        return routing;
    }

    private void adjustForScheduledSend(Envelope[] outgoing)
    {
        var now = DateTime.UtcNow;
        for (var i = 0; i < outgoing.Length; i++)
        {
            if (outgoing[i].IsScheduledForLater(now) && !outgoing[i].Sender!.SupportsNativeScheduledSend)
            {
                outgoing[i] = outgoing[i].ForScheduledSend(_durableLocalQueue);
            }
        }
    }

    private MessageTypeRouting routingFor(Envelope envelope)
    {
        return routingFor(DetermineMessageType(envelope));
    }

    private MessageTypeRouting routingFor(Type messageType)
    {
        if (messageType == null)
        {
            throw new ArgumentNullException(nameof(messageType));
        }

        if (_routes.TryFind(messageType, out var routing))
        {
            return routing;
        }

        routing = RouteByType(messageType);
        _routes = _routes.AddOrUpdate(messageType, routing);

        return routing;
    }
}
