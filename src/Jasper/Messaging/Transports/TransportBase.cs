using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase<TEndpoint> : ITransport where TEndpoint : Endpoint, new()
    {
        private readonly LightweightCache<Uri, TEndpoint> _listeners =
            new LightweightCache<Uri, TEndpoint>(uri =>
            {
                var endpoint = new TEndpoint();
                endpoint.Parse(uri);

                return endpoint;
            });

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }


        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }

        /// <summary>
        /// If ordering matters, this is a transport's
        /// way to ensure that Uri are evaluated consistently
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected virtual Uri canonicizeUri(Uri uri)
        {
            return uri;
        }

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            var endpoints = _subscriptions.GroupBy(x => x.Uri);
            foreach (var endpoint in endpoints)
            {
                var sender = CreateSender(endpoint.Key, root.Settings.Cancellation, root);
                runtime.AddSubscriber(ReplyUri, sender, endpoint.ToArray());
            }
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var settings in _listeners)
            {
                var listener = createListener(settings, root);
                runtime.AddListener(listener, settings);
            }
        }

        public abstract ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root);

        public void Subscribe(Uri uri, Subscription subscription)
        {
            subscription.Uri = uri;
            _subscriptions.Add(subscription);
        }

        public Endpoint ListenTo(Uri uri)
        {
            uri = canonicizeUri(uri);
            var listener = _listeners[uri];

            if (uri.IsDurable())
            {
                listener.IsDurable = true;
            }

            return listener;
        }


        public void Dispose()
        {
        }

        protected abstract IListener createListener(TEndpoint endpoint, IMessagingRoot root);
    }
}
