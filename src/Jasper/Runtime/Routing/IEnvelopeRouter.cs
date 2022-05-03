using System;
using Jasper.Transports.Sending;

namespace Jasper.Runtime.Routing
{
    public interface IEnvelopeRouter
    {
        Envelope[] RouteOutgoingByMessage(object message);
        Envelope[] RouteOutgoingByEnvelope(Envelope original);
        void RouteToDestination(Uri destination, Envelope envelope);
        Envelope[] RouteToTopic(string topicName, Envelope envelope);
        Envelope RouteLocally<T>(T message, string workerQueue);
        Envelope RouteLocally<T>(T message);
    }
}
