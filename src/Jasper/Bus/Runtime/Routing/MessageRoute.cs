using System;
using System.Linq;
using Jasper.Bus.Runtime.Subscriptions.New;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRoute
    {
        public static bool TryToMatch(PublishedMessage published, NewSubscription subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
        {
            throw new NotImplementedException();
        }

        private readonly IMediaWriter _writer;

        public MessageRoute(Type messageType, ModelWriter writer, Uri destination, string contentType)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            _writer = writer[contentType];
            MessageType = messageType.ToTypeAlias();
            DotNetType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public string MessageType { get; }

        public Type DotNetType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }
        public string Publisher { get; set; }
        public string Receiver { get; set; }


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

        public bool MatchesEnvelope(Envelope envelope)
        {
            if (Destination != envelope.Destination) return false;

            return !envelope.AcceptedContentTypes.Any() || envelope.AcceptedContentTypes.Contains(ContentType);
        }
    }
}
