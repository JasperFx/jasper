using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus
{
    public class ChannelGraph : IContentTypeAware, IDisposable, IEnumerable<ChannelNode>
    {
        private readonly ConcurrentDictionary<Uri, ChannelNode> _nodes = new ConcurrentDictionary<Uri, ChannelNode>();

        public readonly List<string> AcceptedContentTypes = new List<string>();
        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
        public string DefaultContentType => AcceptedContentTypes.FirstOrDefault();

        /// <summary>
        /// Used to identify the instance of the running Jasper node
        /// </summary>
        public string Name { get; set; }

        // TODO -- need to make this the default reply channel
        // if it is not explicitly set
        public ChannelNode ControlChannel { get; set; }

        public ChannelNode DefaultChannel { get; set; }

        public ChannelNode this[Uri uri]
        {
            get
            {
                return _nodes.GetOrAdd(uri, key => new ChannelNode(uri));
            }
        }

        public ChannelNode this[string uriString] => this[uriString.ToUri()];

        public ChannelNode AddChannelIfMissing(Uri uri)
        {
            return this[uri];
        }

        public bool HasChannel(Uri uri)
        {
            return _nodes.ContainsKey(uri);
        }

        public void Dispose()
        {
            _nodes.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ChannelNode> GetEnumerator()
        {
            return _nodes.Values.GetEnumerator();
        }


        public ChannelNode TryGetChannel(Uri address)
        {
            ChannelNode node = null;
            _nodes.TryGetValue(address, out node);

            return node;
        }

        public IEnumerable<ChannelNode> IncomingChannelsFor(string scheme)
        {
            return _nodes.Values.Where(x => x.Incoming && x.Uri.Scheme == scheme);
        }

        internal async Task ApplyLookups(IEnumerable<IUriLookup> lookups)
        {
            foreach (var lookup in lookups)
            {
                var matching = _nodes.Values.Where(x => x.Uri.Scheme == lookup.Protocol).ToArray();
                var actuals = await lookup.Lookup(matching.Select(x => x.Uri).ToArray());

                for (int i = 0; i < matching.Length; i++)
                {
                    var node = matching[i];
                    node.Uri = actuals[i];
                    _nodes[node.Uri] = node;
                }
            }
        }

        internal void StartTransports(IHandlerPipeline pipeline, ITransport[] transports)
        {
            var unknowns = _nodes.Values.Distinct().Where(x => transports.All(t => t.Protocol != x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
            {
                throw new UnknownTransportException(unknowns);
            }

            if (ControlChannel != null)
            {
                ControlChannel.MaximumParallelization = 1;
            }

            if (DefaultChannel == null)
            {
                DefaultChannel = IncomingChannelsFor("loopback").FirstOrDefault();
            }

            foreach (var transport in transports)
            {
                transport.Start(pipeline, this);

                _nodes.Values
                    .Where(x => x.Uri.Scheme == transport.Protocol && x.Sender == null)
                    .Each(x => { x.Sender = new NulloSender(transport, x.Uri); });
            }
        }
    }
}
