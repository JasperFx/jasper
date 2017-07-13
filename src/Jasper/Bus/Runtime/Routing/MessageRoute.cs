using System;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRoute
    {
        public MessageRoute(Type messageType, Uri destination, string contentType)
        {
            MessageType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public Type MessageType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }
    }
}