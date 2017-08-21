using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class PublishedMessage
    {
        public PublishedMessage(Type messageType)
        {
            MessageType = messageType.ToTypeAlias();
            DotNetType = messageType;
        }

        [JsonIgnore]
        public Type DotNetType { get; }

        public string MessageType { get; }

        public string ServiceName { get; set; }
        public string[] ContentTypes { get; set; }

        public override string ToString()
        {
            return $"{nameof(DotNetType)}: {DotNetType}, {nameof(ServiceName)}: {ServiceName}, {nameof(ContentTypes)}: {ContentTypes.Join(", ")}";
        }
    }
}
