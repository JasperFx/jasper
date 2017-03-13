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

            foreach (var node in _nodes.Values)
            {
                var scheme = node.Uri.Scheme;
                if (_transports.ContainsKey(scheme))
                {
                    node.Channel = _transports[scheme].CreateChannel(node);
                }
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

        public ChannelNode GetDestinationChannel(Uri uri)
        {
            // Remember that this one has to use the volatile channel
            // idea from fubu
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var transport in _transports.Values)
            {
                transport.Dispose();
            }

            _transports.Clear();

            foreach (var channelNode in _nodes.Values)
            {
                channelNode.Channel.Dispose();
            }

            _nodes.Clear();
        }
    }
}