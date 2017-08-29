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
    public interface IChannelGraph : IEnumerable<ChannelNode>
    {
        IChannel this[Uri uri] { get; }

        IChannel DefaultChannel { get; }
        IChannel ControlChannel { get; }

        string Name { get; }

        string[] ValidTransports { get;}
        IChannel TryGetChannel(Uri address);
        bool HasChannel(Uri uri);
    }

    public class ChannelGraph : IDisposable, IChannelGraph
    {
        private readonly ConcurrentDictionary<Uri, ChannelNode> _nodes = new ConcurrentDictionary<Uri, ChannelNode>();

        private bool _locked;


        /// <summary>
        /// Used to identify the instance of the running Jasper node
        /// </summary>
        public string Name { get; set; }

        // TODO -- need to make this the default reply channel
        // if it is not explicitly set
        public ChannelNode ControlChannel { get; set; }

        IChannel IChannelGraph.ControlChannel => ControlChannel;

        public ChannelNode DefaultChannel { get; set; }

        IChannel IChannelGraph.DefaultChannel => DefaultChannel;

        public ChannelNode this[Uri uri]
        {
            get
            {
                return _nodes.GetOrAdd(uri, key => new ChannelNode(uri));
            }
        }

        IChannel IChannelGraph.this[Uri uri] => this[uri];

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

        IChannel IChannelGraph.TryGetChannel(Uri address)
        {
            return TryGetChannel(address);
        }

        public IEnumerable<ChannelNode> IncomingChannelsFor(string scheme)
        {
            return _nodes.Values.Where(x => x.Incoming && x.Uri.Scheme == scheme);
        }

        internal async Task ApplyLookups(UriAliasLookup lookups)
        {
            var all = _nodes.Values.Select(x => x.Destination).ToArray();
            await lookups.ReadAliases(all);

            foreach (var node in _nodes.Values.ToArray())
            {
                var resolved = lookups.Resolve(node.Destination);
                if (resolved == node.Destination) continue;

                node.Uri = resolved;
                _nodes[node.Uri] = node;
            }
        }

        internal void StartTransports(IHandlerPipeline pipeline, ITransport[] transports)
        {
            ValidTransports = transports.Select(x => x.Protocol).ToArray();

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

            _locked = true;
        }

        public string[] ValidTransports { get; private set; } = new string[0];
    }
}
