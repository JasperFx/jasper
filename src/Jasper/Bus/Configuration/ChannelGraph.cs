using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;

namespace Jasper.Bus.Configuration
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

        public ChannelGraph()
        {
        }

        // For testing
        public ChannelGraph(params ITransport[] transports)
        {
            UseTransports(transports);
        }

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
            foreach (var transport in _transports.Values)
            {
                transport.Dispose();
            }

            _transports.Clear();

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

        public async Task<Envelope> Send(Envelope envelope, Uri address, IEnvelopeSerializer serializer, IMessageCallback callback = null)
        {

            var transportScheme = address.Scheme;
            if (_nodes.ContainsKey(address))
            {
                transportScheme = _nodes[address].Uri.Scheme;
            }

            ITransport transport = null;
            if (_transports.TryGetValue(transportScheme, out transport))
            {
                var sending = envelope.Clone();

                var channel = TryGetChannel(address);
                channel?.ApplyModifiers(sending);


                // TODO -- there's a little opportunity here to try to reuse the serialization
                // if you send to more than one channel at a time w/ the same serializer
                if (sending.Data == null || sending.Data.Length == 0)
                {
                    serializer.Serialize(sending, channel);
                }


                sending.AcceptedContentTypes = AcceptedContentTypes.ToArray();
                if (channel != null)
                {
                    await sendToStaticChannel(callback, sending, channel);
                }
                else
                {
                    await sendToDynamicChannel(address, callback, sending, transport);
                }

                return sending;
            }
            else
            {
                throw new InvalidOperationException($"Unrecognized transport scheme '{address.Scheme}'");
            }

        }

        private static async Task sendToDynamicChannel(Uri address, IMessageCallback callback, Envelope sending, ITransport transport)
        {
            sending.Destination = address;
            sending.ReplyUri = transport.DefaultReplyUri();

            if (callback == null)
            {
                await transport.Send(sending, sending.Destination).ConfigureAwait(false);
            }
            else
            {
                await callback.Send(sending).ConfigureAwait(false);
            }
        }

        private static async Task sendToStaticChannel(IMessageCallback callback, Envelope sending, ChannelNode channel)
        {
            sending.Destination = channel.Destination;
            sending.ReplyUri = channel.ReplyUri;

            if (callback == null)
            {
                await channel.Sender.Send(sending).ConfigureAwait(false);
            }
            else
            {
                await callback.Send(sending).ConfigureAwait(false);
            }
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

        internal void ApplyLookups(IEnumerable<IUriLookup> lookups)
        {
            foreach (var lookup in lookups)
            {
                var matching = _nodes.Values.Where(x => x.Uri.Scheme == lookup.Protocol).ToArray();

                foreach (var node in matching)
                {
                    node.Uri = lookup.Lookup(node.Uri);
                    _nodes[node.Uri] = node;
                }
            }
        }
    }
}
