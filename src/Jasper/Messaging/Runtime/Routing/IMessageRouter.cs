using System;

namespace Jasper.Messaging.Runtime.Routing
{
    public interface IMessageRouter
    {
        void ClearAll();
        MessageRoute[] Route(Type messageType);
        MessageRoute RouteForDestination(Envelope envelopeDestination);

        Envelope[] Route(Envelope envelope);
        void ApplyMessageTypeSpecificRules(Envelope envelope);
    }
}
