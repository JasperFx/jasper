using System;
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

        public override string ToString()
        {
            return $"{nameof(DotNetType)}: {DotNetType}, {nameof(ServiceName)}: {ServiceName}, {nameof(ContentTypes)}: {ContentTypes.Join(", ")}";
        }
    }
}
