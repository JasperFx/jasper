using System;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class PublishedMessage
    {
        /// <summary>
        /// For serialization only
        /// </summary>
        public PublishedMessage()
        {
        }

        public PublishedMessage(Type messageType)
        {
            MessageType = messageType.ToMessageAlias();
            DotNetType = messageType;
        }

        public PublishedMessage(Type messageType, ModelWriter modelWriter, IChannelGraph channels) : this(messageType)
        {
            ContentTypes = modelWriter.ContentTypes;
            Transports = channels.ValidTransports;
        }

        [JsonIgnore]
        public Type DotNetType { get; }

        public string MessageType { get; set; }

        public string ServiceName { get; set; }
        public string[] ContentTypes { get; set; }
        public string[] Transports { get; set; }

        protected bool Equals(PublishedMessage other)
        {
            return string.Equals(MessageType, other.MessageType) && string.Equals(ServiceName, other.ServiceName) && ContentTypes.SequenceEqual(other.ContentTypes) && Transports.SequenceEqual(other.Transports);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PublishedMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServiceName != null ? ServiceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContentTypes != null ? ContentTypes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Transports != null ? Transports.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(MessageType)}: {MessageType}, {nameof(ServiceName)}: {ServiceName}, {nameof(ContentTypes)}: {ContentTypes}, {nameof(Transports)}: {Transports}";
        }
    }
}
