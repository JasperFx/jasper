using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class CapabilityGraph
    {
        private readonly IList<Type> _publishedMessages = new List<Type>();

        // TODO -- also deal w/ HTTP API endpoints
        public void Compile(ChannelGraph channels, SerializationGraph serializers)
        {
            // look through all the published

            PublishedMessages = _publishedMessages.Select(type =>
            {
                var writer = serializers.WriterFor(type);
                return new Publishing(type, writer.ContentTypes);
            }).ToArray();
        }

        public Publishing[] PublishedMessages { get; private set; }

        public void Publishes(Type type)
        {
            _publishedMessages.Add(type);
        }
    }

    public class Publishing
    {
        public Publishing()
        {
        }

        public Publishing(Type messageType, string[] supportedContentTypes)
        {
            DotNetType = messageType;
            MessageType = messageType.ToTypeAlias();
            SupportedContentTypes = supportedContentTypes;
        }

        [JsonIgnore]
        public Type DotNetType { get; }

        public string MessageType { get; set; }
        public string[] SupportedContentTypes { get; set; }
    }

    public class RequestHandler
    {
        public MessageVersion Request { get; set; }
        public MessageVersion Response { get; set; }

        public Uri Destination { get; set; }

        public string ServiceName { get; set; }
    }

    public class MessageVersion
    {
        public string MessageType { get; set; }
        public string[] SupportedContentTypes { get; set; }
    }

    // Just use Subscription
    public class Subscriber
    {
        public Uri Location { get; set; }
        public string MessageType { get; set; }
        public string[] SupportedContentTypes { get; set; }

        public Uri[] Channels { get; set; }

        public string ServiceName { get; set; }
    }
}
