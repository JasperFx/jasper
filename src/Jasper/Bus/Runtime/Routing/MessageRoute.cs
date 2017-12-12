using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Routing
{
    public class MessageRoute
    {
        public MessageRoute(Type messageType, Uri destination, string contentType)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            MessageType = messageType.ToMessageAlias();
            DotNetType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public MessageRoute(Type messageType, ModelWriter writer, IChannel channel, string contentType)
            : this(messageType, channel.Uri, contentType)
        {
            Writer = writer[contentType];
            Channel = channel;
        }

        public IMessageSerializer Writer { get; internal set; }

        public string MessageType { get; }

        public Type DotNetType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }
        public string Publisher { get; set; }
        public string Receiver { get; set; }
        public IChannel Channel { get; set; }


        public Envelope CloneForSending(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");
            }

            var sending = envelope.Clone();
            sending.Id = CombGuidIdGeneration.NewGuid();
            sending.OriginalId = envelope.Id;

            if (envelope.RequiresLocalReply)
            {
                sending.ReplyUri = envelope.ReplyUri ?? Channel.LocalReplyUri;
            }

            Channel.ApplyModifications(sending);

            sending.ContentType = envelope.ContentType ?? ContentType;

            sending.Writer = Writer;
            sending.Destination = Destination;
            sending.Route = this;

            return sending;
        }

        public bool MatchesEnvelope(Envelope envelope)
        {
            if (Destination != envelope.Destination) return false;

            if (envelope.ContentType != null) return ContentType == envelope.ContentType;

            return !envelope.AcceptedContentTypes.Any() || envelope.AcceptedContentTypes.Contains(ContentType);
        }

        public static bool TryToRoute(PublishedMessage sender, Subscription receiver, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
        {
            route = null;
            mismatch = null;

            var transportsMatch = (sender.Transports ?? new string[0]).Contains(receiver.Destination.Scheme);

            var contentType = SelectContentType(sender, receiver);

            if (transportsMatch && contentType.IsNotEmpty())
            {
                route = new MessageRoute(sender.DotNetType, receiver.Destination, contentType)
                {
                    Publisher = sender.ServiceName,
                    Receiver = receiver.ServiceName
                };
                return true;
            }

            mismatch = new PublisherSubscriberMismatch(sender, receiver)
            {
                IncompatibleTransports = !transportsMatch,
                IncompatibleContentTypes = contentType == null
            };

            return false;

        }

        private static string SelectContentType(PublishedMessage sender, Subscription receiver)
        {
            var matchingContentTypes = receiver.Accept.Intersect(sender.ContentTypes).ToArray();
            // Always try to use the versioned or specific reader/writer if one exists
            var contentType = matchingContentTypes.FirstOrDefault(x => x != "application/json")
                              ?? matchingContentTypes.FirstOrDefault();
            return contentType;
        }
    }
}
