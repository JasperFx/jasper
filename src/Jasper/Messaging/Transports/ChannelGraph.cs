using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class ChannelGraph : IChannelGraph, IDisposable
    {
        private MessagingSettings _settings;



        private ImHashMap<Uri, IChannel> _channels = ImHashMap<Uri, IChannel>.Empty;
        private readonly object _channelLock = new object();

        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private UriAliasLookup _lookups;
        private IMessageLogger _logger;
        private IMessagingRoot _root;

        public void Start(IMessagingRoot root)
        {
            var settings = root.Settings;

            _lookups = root.Lookup;
            _settings = settings;
            _logger = root.Logger;
            _root = root;

            organizeTransports(settings, root.Transports);

            assertNoUnknownTransportsInSubscribers(settings);


            assertNoUnknownTransportsInListeners(settings);

            foreach (var transport in root.Transports)
            {
                transport.StartListening(root);
            }

            buildInitialSendingAgents(root);



            GetOrBuildChannel(TransportConstants.RetryUri);

            SystemReplyUri =
                tryGetReplyUri("tcp")
                ?? _transports.Values.FirstOrDefault(x => x.LocalReplyUri != null)?.LocalReplyUri;
        }

        private void buildInitialSendingAgents(IMessagingRoot root)
        {
            foreach (var subscriberAddress in root.Settings.KnownSubscribers)
            {
                var transport = _transports[subscriberAddress.Uri.Scheme];
                var agent = transport.BuildSendingAgent(subscriberAddress.Uri, root, _settings.Cancellation);

                var channel = new Channel(_logger, subscriberAddress, transport.LocalReplyUri, agent);


                _channels = _channels.AddOrUpdate(subscriberAddress.Uri, channel);
            }
        }

        private void organizeTransports(MessagingSettings settings, ITransport[] transports)
        {
            transports.Where(x => settings.StateFor(x.Protocol) != TransportState.Disabled)
                .Each(t => _transports.Add(t.Protocol, t));

            // Super duper hokey
            if (_transports.ContainsKey("http") && !_transports.ContainsKey("https"))
            {
                _transports["https"] = _transports["http"];
            }
        }

        private Uri tryGetReplyUri(string protocol)
        {
            if (_settings.StateFor(protocol) == TransportState.Disabled) return null;

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
            var agent = transport.BuildSendingAgent(uri, _root, _settings.Cancellation);
            return new Channel(_logger, agent, transport.LocalReplyUri);
        }


        public void Dispose()
        {
            foreach (var channel in _channels.Enumerate())
            {
                channel.Value.Dispose();
            }


            _channels = ImHashMap<Uri, IChannel>.Empty;
        }


        public IChannel GetOrBuildChannel(Uri address)
        {
            var uri = _lookups == null ? address : _lookups.Resolve(address);

            assertValidTransport(uri);

            if (_channels.TryFind(uri, out var channel)) return channel;

            lock (_channelLock)
            {
                if (!_channels.TryFind(uri, out channel))
                {
                    channel = buildChannel(address);
                    _channels = _channels.AddOrUpdate(address, channel);
                }

                return channel;
            }

        }

        public bool HasChannel(Uri uri)
        {
            return _channels.TryFind(uri, out var channel);
        }

        public IChannel[] AllKnownChannels()
        {
            return _channels.Enumerate().Select(x => x.Value).ToArray();
        }

        public Uri SystemReplyUri { get; private set; }

        private void assertNoUnknownTransportsInListeners(MessagingSettings settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Scheme)).ToArray();

            if (unknowns.Any())
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.ToString()).Join(", ")}");
            }
        }

        private void assertNoUnknownTransportsInSubscribers(MessagingSettings settings)
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
