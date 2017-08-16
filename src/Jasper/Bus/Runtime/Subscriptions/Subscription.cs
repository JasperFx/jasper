using System;
using Baseline;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class Subscription
    {
        public static Subscription For<T>()
        {
            return new Subscription(typeof(T));
        }

        public Subscription(Type messageType)
        {
            MessageType = messageType.ToTypeAlias();
        }

        public Subscription()
        {
        }

        public Guid Id { get; set; }
        public Uri Source { get; set; }
        public Uri Destination { get; set; }

        public string MessageType { get; set; }

        /// <summary>
        /// Service name of the publishing application
        /// </summary>
        public string Publisher { get; set; }
        public SubscriptionRole Role { get; set; }

        public string Accepts { get; set; } = "application/json";

        public Subscription Clone()
        {
            var clone = (Subscription)this.MemberwiseClone();
            clone.Id = Guid.Empty;

            return clone;
        }

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
            return Equals(Source, other.Source) && Equals(Destination, other.Destination) && string.Equals(MessageType, other.MessageType) && string.Equals(Publisher, other.Publisher) && string.Equals(Role, other.Role);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Destination != null ? Destination.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Publisher != null ? Publisher.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Source: {Source}, Receiver: {Destination}, MessageType: {MessageType}, NodeName: {Publisher}";
        }

        public bool Matches(Type inputType)
        {
            return inputType.GetFullName() == MessageType;
        }
    }
}
