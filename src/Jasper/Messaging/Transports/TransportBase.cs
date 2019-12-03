using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase<TEndpoint> : ITransport where TEndpoint : Endpoint, new()
    {
        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }

        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var endpoint in endpoints().Where(x => x.Subscriptions.Any()))
                endpoint.StartSending(root, runtime, ReplyUri);
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var endpoint in endpoints()) endpoint.StartListening(root, runtime);
        }

        public Endpoint ListenTo(Uri uri)
        {
            uri = canonicizeUri(uri);
            var endpoint = findEndpointByUri(uri);
            endpoint.IsListener = true;

            if (uri.IsDurable()) endpoint.IsDurable = true;

            return endpoint;
        }


        public void Dispose()
        {
        }

        protected abstract IEnumerable<TEndpoint> endpoints();

        /// <summary>
        ///     If ordering matters, this is a transport's
        ///     way to ensure that Uri are evaluated consistently
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected virtual Uri canonicizeUri(Uri uri)
        {
            return uri;
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            var shouldBeDurable = uri.IsDurable();

            var endpoint = findEndpointByUri(canonicizeUri(uri));

            // It's coded this way so you don't override
            // durability if it's already set
            if (shouldBeDurable)
            {
                endpoint.IsDurable = true;
            }

            return endpoint;
        }

        protected abstract TEndpoint findEndpointByUri(Uri uri);
    }
}
