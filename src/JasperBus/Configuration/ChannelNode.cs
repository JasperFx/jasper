using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JasperBus.Runtime;
using JasperBus.Runtime.Routing;

namespace JasperBus.Configuration
{
    public class ChannelNode : IContentTypeAware
    {
        public Uri Uri { get; }

        public ChannelNode(Uri uri)
        {
            Uri = uri;
        }

        public readonly List<string> AcceptedContentTypes = new List<string>();

        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
        public bool Incoming { get; set; }
        public string DefaultContentType => AcceptedContentTypes.FirstOrDefault();

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Rules.Any(x => x.Matches(messageType));
        }
    }

    internal interface IContentTypeAware
    {
        IEnumerable<string> Accepts { get; }
    }
}