using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase<TEndpoint> : ITransport where TEndpoint : Endpoint, new()
    {

        [Obsolete]
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();



        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }

        protected abstract IEnumerable<TEndpoint> endpoints();

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
            foreach (var endpoint in endpoints())
            {
                endpoint.StartListening(root, runtime);
            }
        }

        public abstract ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root);

        public void Subscribe(Uri uri, Subscription subscription)
        {
            subscription.Uri = canonicizeUri(uri);
            _subscriptions.Add(subscription);
        }

        public Endpoint ListenTo(Uri uri)
        {
            uri = canonicizeUri(uri);
            var endpoint = findEndpointByUri(uri);

            if (uri.IsDurable())
            {
                endpoint.IsDurable = true;
            }

            return endpoint;
        }

        protected abstract TEndpoint findEndpointByUri(Uri uri);


        public void Dispose()
        {
        }

    }
}
