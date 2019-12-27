using System;
using System.Linq;
using Jasper.Serialization;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime.Routing
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

        public MessageRoute(Type messageType, WriterCollection<IMessageSerializer> writerCollection, ISendingAgent sender, string contentType)
            : this(messageType, sender.Destination, contentType)
        {
            Writer = writerCollection[contentType];
            Sender = sender;
        }

        public MessageRoute(Envelope envelope, ISendingAgent sender)
        {
            if (envelope.Destination == null) throw new ArgumentNullException(nameof(envelope.Destination));

            MessageType = envelope.MessageType;
            Sender = sender;
        }

        public IMessageSerializer Writer { get; internal set; }

        public string MessageType { get; }

        public Type DotNetType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }

        public ISendingAgent Sender { get; set; }


        public Envelope CloneForSending(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");

            var sending = envelope.CloneForWriter(Writer);
            sending.Id = CombGuidIdGeneration.NewGuid();
            sending.CorrelationId = envelope.Id;

            sending.ReplyUri = envelope.ReplyUri ?? Sender.ReplyUri;

            sending.ContentType = envelope.ContentType ?? ContentType;

            sending.Destination = Destination;

            sending.Sender = Sender;

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
