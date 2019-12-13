using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase<TEndpoint> : ITransport where TEndpoint : Endpoint, new()
    {
        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }

        public string Protocol { get; }

        public IEnumerable<Endpoint> Endpoints()
        {
            return endpoints();
        }

        public virtual void Initialize()
        {
            // Nothing
        }

        public Endpoint ReplyEndpoint()
        {
            var listeners = endpoints().Where(x => x.IsListener).ToArray();


            switch (listeners.Length)
            {
                case 0:
                    return null;

                case 1:
                    return listeners.Single();

                default:
                    return listeners.FirstOrDefault(x => x.IsUsedForReplies) ?? listeners.First();
            }

        }

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            var replyUri = ReplyEndpoint()?.ReplyUri();

            foreach (var endpoint in endpoints().Where(x => x.Subscriptions.Any()))
            {
                endpoint.StartSending(root, runtime, replyUri);
            }
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

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            var shouldBeDurable = uri.IsDurable();

            var endpoint = findEndpointByUri(canonicizeUri(uri));

            // It's coded this way so you don't override
            // durability if it's already set
            if (shouldBeDurable) endpoint.IsDurable = true;

            return endpoint;
        }

        public Endpoint TryGetEndpoint(Uri uri)
        {
            return findEndpointByUri(canonicizeUri(uri));
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

        protected abstract TEndpoint findEndpointByUri(Uri uri);
    }
}
