using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class TransportUri
    {
        public TransportUri(Uri uri)
        {
            Protocol = uri.Scheme;
            ConnectionName = uri.Host;

            var segments = new Queue<string>(uri.Segments.Where(x => x != "/").Select(x => x.Trim('/')));

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
                    TopicName = value;
                    break;

                case TransportConstants.Queue:
                    QueueName = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(uri),
                        $"Invalid Transport Uri 'uri'. Unable to read queue and/or topic");
            }
        }

        public TransportUri(string uriString) : this(uriString.ToUri())
        {
        }

        public TransportUri(string protocol, string connectionName, bool durable, string queueName = null, string topicName = null)
        {
            Protocol = protocol;
            TopicName = topicName;
            QueueName = queueName;
            ConnectionName = connectionName;
            Durable = durable;
        }

        public string Protocol { get; }
        public string TopicName { get; private set; }
        public string QueueName { get; private set; }
        public string ConnectionName { get; }
        public bool Durable { get; }

        public Uri ToUri()
        {
            var uriString = $"{Protocol}://{ConnectionName}";
            if (Durable)
            {
                uriString += "/durable";
            }

            if (QueueName.IsNotEmpty())
            {
                uriString += $"/queue/{QueueName}";
            }

            if (TopicName.IsNotEmpty())
            {
                uriString += $"/topic/{TopicName}";
            }

            return new Uri(uriString);
        }

        public TransportUri ReplaceConnection(string connectionName)
        {
            return new TransportUri(Protocol, connectionName, Durable, QueueName, TopicName);
        }

        protected bool Equals(TransportUri other)
        {
            return string.Equals(Protocol, other.Protocol) && string.Equals(TopicName, other.TopicName) && string.Equals(QueueName, other.QueueName) && string.Equals(ConnectionName, other.ConnectionName) && Durable == other.Durable;
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
                hashCode = (hashCode * 397) ^ (ConnectionName != null ? ConnectionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Durable.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Protocol)}: {Protocol}, {nameof(TopicName)}: {TopicName}, {nameof(QueueName)}: {QueueName}, {nameof(ConnectionName)}: {ConnectionName}, {nameof(Durable)}: {Durable}";
        }
    }
}
