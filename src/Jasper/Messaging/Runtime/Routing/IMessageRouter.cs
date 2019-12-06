using System;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Runtime.Routing
{
    public interface IMessageRouter
    {
        void ClearAll();
        MessageRoute[] Route(Type messageType);
        MessageRoute RouteForDestination(Envelope envelopeDestination);

        Envelope[] Route(Envelope envelope);
        void ApplyMessageTypeSpecificRules(Envelope envelope);
        MessageRoute CreateLocalRoute(Type messageType);
        ISendingAgent LocalQueueByMessageType(Type messageType);
    }
}
