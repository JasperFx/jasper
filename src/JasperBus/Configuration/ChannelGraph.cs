using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JasperBus.Configuration
{
    public class ChannelGraph : IContentTypeAware
    {
        private readonly ConcurrentDictionary<Uri, ChannelNode> _nodes = new ConcurrentDictionary<Uri, ChannelNode>();

        public readonly List<string> AcceptedContentTypes = new List<string>();
        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
    }
}