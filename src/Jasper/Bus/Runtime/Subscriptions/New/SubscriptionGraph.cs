using System;
using System.Collections.Generic;
using Jasper.Conneg;
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

        public IList<string> ContentTypes { get; } = new List<string>();


    }

    public class SubscriptionGraph
    {
        private readonly IList<PublishedMessage> _published = new List<PublishedMessage>();

        public void Publish(Type messageType)
        {
            _published.Add(new PublishedMessage(messageType));
        }

        public void Subscribe(Type messageType)
        {

        }

        public void Compile(SerializationGraph serialization, ChannelGraph channels)
        {
            // MAY NEED TO USE IUriLookups here
            throw new NotImplementedException();
        }
    }

    public class SubscriptionRequirement
    {
        public SubscriptionRequirement(Type messageType)
        {
        }
    }
}
