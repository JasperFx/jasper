using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Util;

namespace Jasper.Transports
{
    public abstract class TransportBase<TEndpoint> : ITransport where TEndpoint : Endpoint
    {
        public TransportBase(string protocol, string name)
        {
            Protocols.Add(protocol);
            Name = name;
        }

        public TransportBase(IEnumerable<string> protocols, string name)
        {
            foreach (string protocol in protocols)
            {
                Protocols.Add(protocol);
            }

            Name = name;
        }

        public string Name { get; }

        public ICollection<string> Protocols { get; } = new List<string>();

        public IEnumerable<Endpoint> Endpoints()
        {
            return endpoints();
        }

        public virtual ValueTask InitializeAsync(IJasperRuntime root)
        {
            // Nothing
            return ValueTask.CompletedTask;
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

        public void StartSenders(IJasperRuntime root)
        {
            var replyUri = ReplyEndpoint()?.ReplyUri();

            foreach (var endpoint in endpoints().Where(x => x.Subscriptions.Any()))
            {
                endpoint.StartSending(root, replyUri);
            }
        }

        public void StartListeners(IJasperRuntime root)
        {
            foreach (var endpoint in endpoints()) endpoint.StartListening(root);
        }

        public Endpoint ListenTo(Uri? uri)
        {
            uri = canonicizeUri(uri);
            var endpoint = findEndpointByUri(uri);
            endpoint.IsListener = true;

            if (uri.IsDurable()) endpoint.Mode = EndpointMode.Durable;

            return endpoint;
        }


        public virtual void Dispose()
        {
            foreach (var endpoint in endpoints().OfType<IDisposable>())
            {
                endpoint.Dispose();
            }
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            var shouldBeDurable = uri.IsDurable();

            var endpoint = findEndpointByUri(canonicizeUri(uri));

            // It's coded this way so you don't override
            // durability if it's already set
            if (shouldBeDurable) endpoint.Mode = EndpointMode.Durable;

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
        protected virtual Uri? canonicizeUri(Uri? uri)
        {
            return uri;
        }

        protected abstract TEndpoint findEndpointByUri(Uri uri);
    }
}
