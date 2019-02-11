using System;
using System.Collections.Generic;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class ExternalTransportSettings<TEndpoint> where TEndpoint : class
    {
        private ImHashMap<TransportUri, TEndpoint> _endpoints = ImHashMap<TransportUri,TEndpoint>.Empty;

        private readonly object _locker = new object();

        public Dictionary<string, string> Connections { get; set; } = new Dictionary<string, string>();

        protected abstract TEndpoint buildEndpoint(TransportUri uri, string connectionString);

        public void ConfigureEndpoint(string uriString, Action<TEndpoint> alteration)
        {
            ConfigureEndpoint(new TransportUri(uriString), alteration);
        }
        
        public void ConfigureEndpoint(Uri uri, Action<TEndpoint> alteration)
        {
            ConfigureEndpoint(new TransportUri(uri), alteration);
        }

        public void ConfigureEndpoint(TransportUri uri, Action<TEndpoint> alteration)
        {
            var endpoint = For(uri);
            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown connection named '{uri.ConnectionName}'");

            alteration(endpoint);
        }

        public TEndpoint For(string uri)
        {
            return For(new TransportUri(uri));
        }

        public TEndpoint For(TransportUri uri)
        {
            if (_endpoints.TryFind(uri, out var endpoint))
            {
                return endpoint;
            }

            lock (_locker)
            {
                if (_endpoints.TryFind(uri, out endpoint))
                {
                    return endpoint;
                }

                if (!Connections.ContainsKey(uri.ConnectionName)) return null;

                endpoint = buildEndpoint(uri, Connections[uri.ConnectionName]);
                _endpoints = _endpoints.AddOrUpdate(uri, endpoint);

                return endpoint;

            }
        }

        // TODO -- use the service name if you can?
        public TransportUri ReplyUri { get; set; }
    }
}
