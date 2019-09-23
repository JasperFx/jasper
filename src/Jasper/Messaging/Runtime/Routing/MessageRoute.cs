using System;
using System.Linq;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Routing
{
    public class MessageRoute
    {
        public MessageRoute(Type messageType, Uri destination, string contentType)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            MessageType = messageType.ToMessageTypeName();
            DotNetType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public MessageRoute(Type messageType, ModelWriter writer, ISubscriber subscriber, string contentType)
            : this(messageType, subscriber.Uri, contentType)
        {
            Writer = writer[contentType];
            Subscriber = subscriber;
        }

        public MessageRoute(Envelope envelope, ISubscriber subscriber)
        {
            if (envelope.Destination == null) throw new ArgumentNullException(nameof(envelope.Destination));

            MessageType = envelope.MessageType;
            Subscriber = subscriber;
        }

        public IMessageSerializer Writer { get; internal set; }

        public string MessageType { get; }

        public Type DotNetType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }

        public ISubscriber Subscriber { get; set; }


        public Envelope CloneForSending(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");

            var sending = envelope.Clone(Writer);
            sending.Id = CombGuidIdGeneration.NewGuid();
            sending.CorrelationId = envelope.Id;

            sending.Data = envelope.Data;

            sending.ReplyUri = envelope.ReplyUri ?? Subscriber.ReplyUri;

            sending.ContentType = envelope.ContentType ?? ContentType;

            sending.Destination = Destination;

            sending.Subscriber = Subscriber;

            return sending;
        }

        public bool MatchesEnvelope(Envelope envelope)
        {
            if (Destination != envelope.Destination) return false;

            if (envelope.ContentType != null) return ContentType == envelope.ContentType;

            return !envelope.AcceptedContentTypes.Any() || envelope.AcceptedContentTypes.Contains(ContentType);
        }

        public override string ToString()
        {
            return
                $"{nameof(MessageType)}: {MessageType}, {nameof(DotNetType)}: {DotNetType}, {nameof(Destination)}: {Destination}, {nameof(ContentType)}: {ContentType}";
        }
    }
}
