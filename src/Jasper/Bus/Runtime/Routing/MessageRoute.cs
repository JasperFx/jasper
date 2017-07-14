using System;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRoute
    {
        private readonly IMediaWriter _writer;

        public MessageRoute(Type messageType, ModelWriter writer, Uri destination, string contentType)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            _writer = writer[contentType];
            MessageType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public Type MessageType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }


        public Envelope CloneForSending(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");
            }

            var sending = envelope.Clone();

            sending.ContentType = envelope.ContentType ?? ContentType;
            sending.Data = _writer.Write(envelope.Message);
            sending.Destination = Destination;

            return sending;
        }
    }
}
