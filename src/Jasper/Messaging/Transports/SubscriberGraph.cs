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
    public class SubscriberGraph : ISubscriberGraph, IDisposable
    {
        private MessagingSettings _settings;



        private ImHashMap<Uri, ISubscriber> _subscribers = ImHashMap<Uri, ISubscriber>.Empty;
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



            GetOrBuild(TransportConstants.RetryUri);
        }

        private void buildInitialSendingAgents(IMessagingRoot root)
        {
            foreach (var subscriberAddress in root.Settings.KnownSubscribers)
            {
                var transport = _transports[subscriberAddress.Uri.Scheme];
                var agent = transport.BuildSendingAgent(subscriberAddress.Uri, root, _settings.Cancellation);

                subscriberAddress.StartSending(_logger, agent, transport.ReplyUri);


                _subscribers = _subscribers.AddOrUpdate(subscriberAddress.Uri, subscriberAddress);
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


        public string[] ValidTransports => _transports.Keys.ToArray();

        private void assertValidTransport(Uri uri)
        {
            if (!_transports.ContainsKey(uri.Scheme))
            {
                throw new ArgumentOutOfRangeException(nameof(uri), $"Unrecognized transport scheme '{uri.Scheme}'");
            }
        }

        private ISubscriber buildChannel(Uri uri)
        {
            assertValidTransport(uri);

            var transport = _transports[uri.Scheme];
            var agent = transport.BuildSendingAgent(uri, _root, _settings.Cancellation);

            var subscriber = new Subscriber(uri);
            subscriber.StartSending(_logger, agent, transport.ReplyUri);

            return subscriber;
        }


        public void Dispose()
        {
            foreach (var channel in _subscribers.Enumerate())
            {
                channel.Value.Dispose();
            }


            _subscribers = ImHashMap<Uri, ISubscriber>.Empty;
        }


        public ISubscriber GetOrBuild(Uri address)
        {
            var uri = _lookups == null ? address : _lookups.Resolve(address);

            assertValidTransport(uri);

            if (_subscribers.TryFind(uri, out var channel)) return channel;

            lock (_channelLock)
            {
                if (!_subscribers.TryFind(uri, out channel))
                {
                    channel = buildChannel(address);
                    _subscribers = _subscribers.AddOrUpdate(address, channel);
                }

                return channel;
            }

        }

        public bool HasSubscriber(Uri uri)
        {
            return _subscribers.TryFind(uri, out var channel);
        }

        public ISubscriber[] AllKnown()
        {
            return _subscribers.Enumerate().Select(x => x.Value).ToArray();
        }

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
