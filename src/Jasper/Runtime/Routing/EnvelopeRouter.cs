using System;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Runtime.Routing
{
    public class EnvelopeRouter : IEnvelopeRouter
    {
        private readonly IMessagingRoot _root;
        private ImHashMap<Type, MessageTypeRouting> _routes = ImHashMap<Type, MessageTypeRouting>.Empty;
        private readonly ISendingAgent _durableLocalQueue;

        public EnvelopeRouter(IMessagingRoot root)
        {
            _root = root;
            _durableLocalQueue = root.Runtime.GetOrBuildSendingAgent(TransportConstants.DurableLocalUri);
        }

        public Envelope[] RouteOutgoingByMessage(object message)
        {
            var envelopes = routingFor(message.GetType()).RouteByMessage(message);
            adjustForScheduledSend(envelopes);

            return envelopes;
        }

        public MessageTypeRouting RouteByType(Type messageType)
        {
            var routing = new MessageTypeRouting(messageType, _root);

            var subscribers = _root.Runtime
                .FindSubscribersForMessageType(messageType);


            if (subscribers.Any())
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.AddRoute(routing, _root);
                }
            }
            else if (_root.Options.HandlerGraph.CanHandle(messageType))
            {
                routing.UseLocalQueueAsRoute();
            }

            return routing;
        }

        private void adjustForScheduledSend(Envelope[] outgoing)
        {
            var now = DateTime.UtcNow;
            for (int i = 0; i < outgoing.Length; i++)
            {
                if (outgoing[i].IsDelayed(now) && !outgoing[i].Sender.SupportsNativeScheduledSend)
                {
                    outgoing[i] = outgoing[i].ForScheduledSend(_durableLocalQueue);
                }
            }
        }

        public Envelope[] RouteOutgoingByEnvelope(Envelope original)
        {
            var messageType = determineMessageType(original);

            var messageTypeRouting = routingFor(messageType);

            Envelope[] envelopes = original.TopicName.IsEmpty()
                ? messageTypeRouting.RouteByEnvelope(messageType, original)
                : messageTypeRouting.RouteToTopic(messageType, original);

            adjustForScheduledSend(envelopes);

            return envelopes;
        }

        private Type determineMessageType(Envelope original)
        {
            return original.Message?.GetType()
                   ?? _root.Serialization.ReaderFor(original.MessageType)?.MessageType;
        }

        public void RouteToDestination(Uri destination, Envelope envelope)
        {
            routingFor(envelope).RouteToDestination(envelope);
        }

        private MessageTypeRouting routingFor(Envelope envelope)
        {
            return routingFor(determineMessageType(envelope));
        }

        public Envelope[] RouteToTopic(string topicName, Envelope envelope)
        {
            var messageTypeRouting = routingFor(envelope);
            envelope.TopicName = topicName;
            var envelopes = messageTypeRouting.RouteToTopic(messageTypeRouting.MessageType, envelope);

            adjustForScheduledSend(envelopes);

            return envelopes;
        }

        Envelope IEnvelopeRouter.RouteLocally<T>(T message)
        {
            var agent = routingFor(typeof(T)).LocalQueue;

            // TODO -- attach the Json serializer or later MessagePack
            // top avoid an extra lookup
            return new Envelope(message)
            {
                Destination = agent.Destination,
                ContentType = "application/json",
                Sender = agent,
                Writer = _root.Serialization.JsonWriterFor(typeof(T))
            };
        }

        public Envelope RouteLocally<T>(T message, string workerQueue)
        {
            var agent = _root.Runtime.AgentForLocalQueue(workerQueue);

            // TODO -- attach the Json serializer or later MessagePack
            // top avoid an extra lookup
            return new Envelope(message)
            {
                Destination = agent.Destination,
                ContentType = "application/json",
                Sender = agent,
                Writer = _root.Serialization.JsonWriterFor(typeof(T))
            };
        }

        private MessageTypeRouting routingFor(Type messageType)
        {
            if (_routes.TryFind(messageType, out var routing))
            {
                return routing;
            }

            routing = RouteByType(messageType);
            _routes = _routes.AddOrUpdate(messageType, routing);

            return routing;
        }


    }
}
