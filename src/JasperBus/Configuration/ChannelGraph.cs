using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperBus.Runtime;

namespace JasperBus.Configuration
{
    public class ChannelGraph : IContentTypeAware, IDisposable
    {
        private readonly ConcurrentDictionary<Uri, ChannelNode> _nodes = new ConcurrentDictionary<Uri, ChannelNode>();

        public readonly List<string> AcceptedContentTypes = new List<string>();
        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
        public string DefaultContentType => AcceptedContentTypes.FirstOrDefault();

        internal void UseTransports(IEnumerable<ITransport> transports)
        {
            foreach (var transport in transports)
            {
                _transports.SmartAdd(transport.Protocol, transport);
            }
        }

        private readonly IDictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public ChannelNode this[Uri uri]
        {
            get
            {
                return _nodes.GetOrAdd(uri, key => new ChannelNode(uri));
            }
        }

        public bool HasChannel(Uri uri)
        {
            return _nodes.ContainsKey(uri);
        }

        public void Dispose()
        {
            foreach (var transport in _transports.Values)
            {
                transport.Dispose();
            }

            _transports.Clear();

            _nodes.Clear();
        }
    }
}