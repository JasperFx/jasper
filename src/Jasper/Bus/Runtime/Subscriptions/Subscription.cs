using System;
using System.Linq;
using System.Net;
using Baseline;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionDelta
    {
        public SubscriptionDelta(Subscription[] expected, Subscription[] actual)
        {
            Matched = expected.Intersect(actual).ToArray();
            NewSubscriptions = expected.Where(x => !Matched.Contains(x)).ToArray();
            ObsoleteSubscriptions = actual.Where(x => !Matched.Contains(x)).ToArray();
        }

        public Subscription[] ObsoleteSubscriptions { get; set; }

        public Subscription[] NewSubscriptions { get; }

        public Subscription[] Matched { get; }
    }

    public class Subscription
    {
        public Subscription(Type messageType, Uri destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            Destination = destination;
            MessageType = messageType?.ToTypeAlias() ?? throw new ArgumentNullException(nameof(messageType));
        }

        public Subscription()
        {
        }

        public string Id
        {
            get
            {
                var destination = WebUtility.UrlDecode(Destination.ToString());
                var contentType = WebUtility.UrlDecode(Accepts);

                return $"{MessageType}/{destination}/{contentType}";
            }
        }

        public Uri Source { get; set; }
        public Uri Destination { get; set; }

        public string MessageType { get; set; }

        /// <summary>
        /// Service name of the publishing application
        /// </summary>
        public string Publisher { get; set; }
        public SubscriptionRole Role { get; set; }

        public string Accepts { get; set; } = "application/json";

        public Subscription SourcedFrom(Uri uri)
        {
            Source = uri;
            return this;
        }

        public Subscription ReceivedBy(Uri uri)
        {
            Destination = uri;
            return this;
        }

        protected bool Equals(Subscription other)
        {
            return Equals(Destination, other.Destination) && string.Equals(MessageType, other.MessageType) && string.Equals(Accepts, other.Accepts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Destination != null ? Destination.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Accepts != null ? Accepts.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Source: {Source}, Receiver: {Destination}, MessageType: {MessageType}, NodeName: {Publisher}";
        }

    }
}
