using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Transports
{
    public class ChannelGraph : IChannelGraph, IDisposable
    {
        private BusSettings _settings;
        private readonly ConcurrentDictionary<Uri, Lazy<IChannel>> _channels = new ConcurrentDictionary<Uri, Lazy<IChannel>>();
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private UriAliasLookup _lookups;


        public void Start(BusSettings settings, ITransport[] transports, UriAliasLookup lookups, CapabilityGraph capabilities)
        {
            _lookups = lookups;
            _settings = settings;

            transports.Where(x => settings.StateFor(x.Protocol) != TransportState.Disabled)
                .Each(t => _transports.Add(t.Protocol, t));




            if (settings.DefaultChannelAddress != null)
            {
                DefaultChannel = GetOrBuildChannel(settings.DefaultChannelAddress);
            }

            assertNoUnknownTransportsInSubscribers(settings);


            assertNoUnknownTransportsInListeners(settings);

            foreach (var transport in _transports.Values)
            {
                transport.StartListening(settings);
            }

            foreach (var subscriberAddress in settings.KnownSubscribers)
            {
                var transport = _transports[subscriberAddress.Uri.Scheme];
                var agent = transport.BuildSendingAgent(subscriberAddress.Uri, _settings.Cancellation);

                var channel = new Channel(subscriberAddress, transport.LocalReplyUri, agent);

                _channels[subscriberAddress.Uri] = new Lazy<IChannel>(() => channel);
            }



            GetOrBuildChannel(TransportConstants.RetryUri);

            SystemReplyUri =
                capabilities.DefaultReceiverLocation
                ?? tryGetReplyUri("http")
                ?? tryGetReplyUri("tcp")
                ?? _transports.Values.FirstOrDefault(x => x.LocalReplyUri != null)?.LocalReplyUri;
        }

        private Uri tryGetReplyUri(string protocol)
        {
            return _transports.ContainsKey(protocol) ? _transports[protocol].LocalReplyUri : null;
        }

        public string[] ValidTransports => _transports.Keys.ToArray();

        private void assertValidTransport(Uri uri)
        {
            if (!_transports.ContainsKey(uri.Scheme))
            {
                throw new ArgumentOutOfRangeException(nameof(uri), $"Unrecognized transport scheme '{uri.Scheme}'");
            }
        }

        private IChannel buildChannel(Uri uri)
        {
            assertValidTransport(uri);

            var transport = _transports[uri.Scheme];
            var agent = transport.BuildSendingAgent(uri, _settings.Cancellation);
            return new Channel(agent, transport.LocalReplyUri);
        }

        public IChannel DefaultChannel { get; private set; }

        public void Dispose()
        {
            foreach (var value in _channels.Values)
            {
                if (value.IsValueCreated)
                {
                    value.Value.Dispose();
                }
            }

            _channels.Clear();
        }


        public IChannel GetOrBuildChannel(Uri address)
        {
            var uri = _lookups == null ? address : _lookups.Resolve(address);

            assertValidTransport(uri);

            return _channels.GetOrAdd(uri, u => new Lazy<IChannel>(() => buildChannel(u))).Value;
        }

        public bool HasChannel(Uri uri)
        {
            return _channels.ContainsKey(uri);
        }

        public IChannel[] AllKnownChannels()
        {
            return _channels.Values.Select(x => x.Value).ToArray();
        }

        public Uri SystemReplyUri { get; private set; }

        private void assertNoUnknownTransportsInListeners(BusSettings settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Scheme)).ToArray();

            if (unknowns.Any())
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.ToString()).Join(", ")}");
            }
        }

        private void assertNoUnknownTransportsInSubscribers(BusSettings settings)
        {
            var unknowns = settings.KnownSubscribers.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
            }
        }
    }
}
