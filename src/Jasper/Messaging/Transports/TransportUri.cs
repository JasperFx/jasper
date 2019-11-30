using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    [Obsolete("Is this thing necessary?")]
    public class TransportUri
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public TransportUri(Uri uri)
        {
            if (uri.Host.IsEmpty()) throw new ArgumentOutOfRangeException(nameof(uri), $"{nameof(uri.Scheme)} is required as the connection name");

            Protocol = uri.Scheme;
            ConnectionName = uri.Host;

            var segments = new Queue<string>(uri.Segments.Where(x => x != "/").Select(x => x.Trim('/')));

            if (segments.Count == 0) throw new ArgumentOutOfRangeException($"Incomplete information in '{uri}'");

            if (segments.Peek() == TransportConstants.Durable)
            {
                Durable = true;
                segments.Dequeue();
            }

            while (segments.Any())
            {
                readSegments(segments, uri);
            }

        }

        public TransportUri CloneForTopic(string topicName)
        {
            return new TransportUri(Protocol, ConnectionName, Durable, QueueName, topicName, SubscriptionName, RoutingKey);
        }

        public bool IsMessageSpecificTopic() => TopicName == "*";

        private void readSegments(Queue<string> segments, Uri uri)
        {
            var next = segments.Dequeue();

            if (!segments.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(uri),
                    $"Invalid Transport Uri 'uri'. Unable to read queue and/or topic");
            }

            var value = segments.Dequeue();

            switch (next)
            {
                case TransportConstants.Topic:
                case TransportConstants.Queue:
                case TransportConstants.Subscription:
                case TransportConstants.Routing:
                    _values.SmartAdd(next, value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(uri),
                        $"Invalid Transport Uri 'uri'. Unable to read queue and/or topic");
            }
        }

        public TransportUri(string uriString) : this(uriString.ToUri())
        {
        }

        public TransportUri(string protocol, string connectionName, bool durable, string queueName = null, string topicName = null, string subscriptionName = null, string routingKey = null)
        {
            Protocol = protocol;

            set(TransportConstants.Topic, topicName);
            set(TransportConstants.Queue, queueName);
            set(TransportConstants.Subscription, subscriptionName);
            set(TransportConstants.Routing, routingKey);


            ConnectionName = connectionName;
            Durable = durable;
        }

        public string[] UriKeys()
        {
            return _values.Keys.ToArray();
        }

        private void set(string key, string value)
        {
            if (value.IsEmpty())
            {
                return;
            }

            if (_values.ContainsKey(key))
            {
                _values[key] = value;
            }
            else
            {
                _values.Add(key, value);
            }
        }

        private string get(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        public string Protocol { get; }
        public string TopicName => get(TransportConstants.Topic);
        public string QueueName => get(TransportConstants.Queue);

        public string SubscriptionName => get(TransportConstants.Subscription);
        public string ConnectionName { get; }
        public bool Durable { get; }

        public string RoutingKey => get(TransportConstants.Routing);

        public Uri ToUri()
        {
            var uriString = $"{Protocol}://{ConnectionName}";

            if (Durable)
            {
                uriString += "/durable";
            }

            if (_values.Any())
            {
                uriString += "/" + _values.Select(x => $"{x.Key}/{x.Value}").Join("/");
            }

            return new Uri(uriString);
        }

        public TransportUri ReplaceConnection(string connectionName)
        {
            return new TransportUri(Protocol, connectionName, Durable, QueueName, TopicName);
        }

        protected bool Equals(TransportUri other)
        {
            return string.Equals(Protocol, other.Protocol) && string.Equals(TopicName, other.TopicName) && string.Equals(QueueName, other.QueueName) && string.Equals(SubscriptionName, other.SubscriptionName) && string.Equals(ConnectionName, other.ConnectionName) && Durable == other.Durable;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransportUri) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Protocol != null ? Protocol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TopicName != null ? TopicName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueueName != null ? QueueName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SubscriptionName != null ? SubscriptionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionName != null ? ConnectionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Durable.GetHashCode();
                return hashCode;
            }
        }
    }
}
