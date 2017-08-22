using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Baseline;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class Subscription
    {
        public Subscription(Type messageType, Uri destination)
        {
            // Okay to let destination be null here.
            Destination = destination;
            MessageType = messageType?.ToTypeAlias() ?? throw new ArgumentNullException(nameof(messageType));
            DotNetType = messageType;
        }

        [JsonIgnore]
        public Type DotNetType { get; }

        // for serialization
        public Subscription()
        {
        }

        public string Id => $"{MessageType}/{WebUtility.UrlDecode(Destination.ToString())}";

        public Uri Destination { get; set; }

        public string MessageType { get; set; }

        public string ServiceName { get; set; }

        private readonly IList<string> _accepts = new List<string>();

        public string[] Accept
        {
            get => _accepts.ToArray();
            set
            {
                _accepts.Clear();
                if (value != null) _accepts.AddRange(value);
            }

        }

        private sealed class DestinationMessageTypeEqualityComparer : IEqualityComparer<Subscription>
        {
            public bool Equals(Subscription x, Subscription y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Equals(x.Destination, y.Destination) && string.Equals(x.MessageType, y.MessageType);
            }

            public int GetHashCode(Subscription obj)
            {
                unchecked
                {
                    return ((obj.Destination != null ? obj.Destination.GetHashCode() : 0) * 397) ^ (obj.MessageType != null ? obj.MessageType.GetHashCode() : 0);
                }
            }
        }

        public static IEqualityComparer<Subscription> DestinationMessageTypeComparer { get; } = new DestinationMessageTypeEqualityComparer();

        public override string ToString()
        {
            return $"{nameof(DotNetType)}: {DotNetType}, {nameof(Destination)}: {Destination}, {nameof(MessageType)}: {MessageType}, {nameof(ServiceName)}: {ServiceName}, {nameof(Accept)}: {Accept}";
        }
    }
}
