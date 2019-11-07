using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class SubscriberGraph : ISubscriberGraph, IDisposable
    {
        private readonly object _channelLock = new object();

        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private IMessageLogger _logger;
        private IMessagingRoot _root;
        private JasperOptions _settings;


        private ImHashMap<Uri, ISubscriber> _subscribers = ImHashMap<Uri, ISubscriber>.Empty;


        public string[] ValidTransports => _transports.Keys.ToArray();


        public void Dispose()
        {
            foreach (var channel in _subscribers.Enumerate()) channel.Value.Dispose();


            _subscribers = ImHashMap<Uri, ISubscriber>.Empty;
        }


        public ISubscriber GetOrBuild(Uri address)
        {
            assertValidTransport(address);

            if (_subscribers.TryFind(address, out var channel)) return channel;

            lock (_channelLock)
            {
                if (!_subscribers.TryFind(address, out channel))
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

        public void Start(IMessagingRoot root)
        {
            var settings = root.Options;

            _settings = settings;
            _logger = root.Logger;
            _root = root;

            organizeTransports(settings, root.Transports);

            assertNoUnknownTransportsInSubscribers(settings);


            assertNoUnknownTransportsInListeners(settings);

            foreach (var transport in root.Transports) transport.StartListening(root);

            buildInitialSendingAgents(root);


            GetOrBuild(TransportConstants.RetryUri);
        }

        private void buildInitialSendingAgents(IMessagingRoot root)
        {
            var groups = root.Options.Subscriptions.GroupBy(x => x.Uri);
            foreach (var group in groups)
            {
                var subscriber = new Subscriber(group.Key, group);
                var transport = _transports[subscriber.Uri.Scheme];
                var agent = transport.BuildSendingAgent(subscriber.Uri, root, _settings.Cancellation);

                subscriber.StartSending(_logger, agent, transport.ReplyUri);


                _subscribers = _subscribers.AddOrUpdate(subscriber.Uri, subscriber);
            }
        }

        private void organizeTransports(JasperOptions settings, ITransport[] transports)
        {
            foreach (var subscription in settings.Subscriptions.Where(x => x.Uri.Scheme == TransportConstants.Durable))
                subscription.Uri = subscription.Uri.ToCanonicalTcpUri();

            transports
                .Each(t => _transports.Add(t.Protocol, t));

            // Super duper hokey
            if (_transports.ContainsKey("http") && !_transports.ContainsKey("https"))
                _transports["https"] = _transports["http"];
        }

        private void assertValidTransport(Uri uri)
        {
            if (!_transports.ContainsKey(uri.Scheme) && uri.Scheme != TransportConstants.Durable)
                throw new ArgumentOutOfRangeException(nameof(uri), $"Unrecognized transport scheme '{uri.Scheme}'");
        }

        private ISubscriber buildChannel(Uri uri)
        {
            if (uri.Scheme == TransportConstants.Durable) uri = uri.ToCanonicalTcpUri();

            assertValidTransport(uri);

            var transport = _transports[uri.Scheme];
            var agent = transport.BuildSendingAgent(uri, _root, _settings.Cancellation);

            var subscriber = new Subscriber(uri, new Subscription[0]);
            subscriber.StartSending(_logger, agent, transport.ReplyUri);

            return subscriber;
        }

        private void assertNoUnknownTransportsInListeners(JasperOptions settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Scheme)).ToArray();

            if (unknowns.Any())
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.ToString()).Join(", ")}");
        }

        private void assertNoUnknownTransportsInSubscribers(JasperOptions settings)
        {
            var unknowns = settings.Subscriptions.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
                throw new UnknownTransportException(
                    $"Unknown transports referenced in {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
        }
    }
}
