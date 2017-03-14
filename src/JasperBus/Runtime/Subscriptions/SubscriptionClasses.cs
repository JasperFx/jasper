using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;

namespace JasperBus.Runtime.Subscriptions
{
    public interface ISubscriptionCache
    {
        void ClearAll();

        void LoadSubscriptions(IEnumerable<Subscription> subscriptions);

        void Remove(Subscription subscription);

        IEnumerable<Subscription> ActiveSubscriptions { get; }
    }

    // What if we said that the control channel has no parallelism?
    public class InMemorySubscriptionCache : ISubscriptionCache
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public void ClearAll()
        {
            _subscriptions.Clear();
        }

        public void LoadSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            subscriptions.Where(x => !_subscriptions.Contains(x)).Each(x => _subscriptions.Add(x));
        }

        public void Remove(Subscription subscription)
        {
            _subscriptions.Remove(subscription);
        }

        public IEnumerable<Subscription> ActiveSubscriptions => _subscriptions.ToArray();
    }

    public class Subscription
    {
        public static Subscription For<T>()
        {
            return new Subscription(typeof(T));
        }

        public Subscription(Type messageType)
        {
            MessageType = messageType.GetFullName();
        }

        public Subscription()
        {
        }

        public Guid Id { get; set; }
        public Uri Source { get; set; }
        public Uri Receiver { get; set; }
        public string MessageType { get; set; }
        public string NodeName { get; set; }
        public SubscriptionRole Role { get; set; }


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
            Receiver = uri;
            return this;
        }

        protected bool Equals(Subscription other)
        {
            return Equals(Source, other.Source) && Equals(Receiver, other.Receiver) && string.Equals(MessageType, other.MessageType) && string.Equals(NodeName, other.NodeName) && string.Equals(Role, other.Role);
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
                hashCode = (hashCode * 397) ^ (Receiver != null ? Receiver.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NodeName != null ? NodeName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Source: {Source}, Receiver: {Receiver}, MessageType: {MessageType}, NodeName: {NodeName}";
        }

        public bool Matches(Type inputType)
        {
            return inputType.GetFullName() == MessageType;
        }
    }

    public enum SubscriptionRole
    {
        Publishes,
        Subscribes
    }
}