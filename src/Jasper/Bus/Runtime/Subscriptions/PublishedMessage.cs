using System;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class PublishedMessage
    {
        public PublishedMessage(Type messageType)
        {
            MessageType = messageType.ToMessageAlias();
            DotNetType = messageType;
        }

        public PublishedMessage(Type messageType, ModelWriter modelWriter, ChannelGraph channels) : this(messageType)
        {
            ContentTypes = modelWriter.ContentTypes;
            Transports = channels.ValidTransports;
        }

        [JsonIgnore]
        public Type DotNetType { get; }

        public string MessageType { get; }

        public string ServiceName { get; set; }
        public string[] ContentTypes { get; set; }
        public string[] Transports { get; set; }

        public override string ToString()
        {
            return $"{nameof(DotNetType)}: {DotNetType}, {nameof(ServiceName)}: {ServiceName}, {nameof(ContentTypes)}: {ContentTypes.Join(", ")}";
        }
    }
}
