using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class ExternalTransportSettings<TEndpoint> where TEndpoint : class
    {
        private readonly string _protocol;
        private readonly string[] _validTransportUriKeys;
        private ImHashMap<TransportUri, TEndpoint> _endpoints = ImHashMap<TransportUri,TEndpoint>.Empty;

        private readonly object _locker = new object();


        protected ExternalTransportSettings(string protocol, params string[] validTransportUriKeys)
        {
            _protocol = protocol;
            _validTransportUriKeys = validTransportUriKeys;
        }

        public Dictionary<string, string> Connections { get; set; } = new Dictionary<string, string>();

        protected abstract TEndpoint buildEndpoint(TransportUri uri, string connectionString);



        /// <summary>
        /// Configure a specific endpoint
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="alteration"></param>
        public void ConfigureEndpoint(string uriString, Action<TEndpoint> alteration)
        {
            ConfigureEndpoint(new TransportUri(uriString), alteration);
        }

        /// <summary>
        /// Configure a specific endpoint
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="alteration"></param>
        public void ConfigureEndpoint(Uri uri, Action<TEndpoint> alteration)
        {
            ConfigureEndpoint(new TransportUri(uri), alteration);
        }

        /// <summary>
        /// Configure a specific endpoint
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="alteration"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ConfigureEndpoint(TransportUri uri, Action<TEndpoint> alteration)
        {
            var endpoint = For(uri);
            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown connection named '{uri.ConnectionName}'");

            alteration(endpoint);
        }

        /// <summary>
        /// Make the same alteration to all endpoints for a named connection
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="alteration"></param>
        public void ConfigureEndpointsForConnection(string connectionName, Action<TEndpoint> alteration)
        {
            foreach (var endpoint in _endpoints.Enumerate().Where(x => x.Key.ConnectionName.EqualsIgnoreCase(connectionName)).Select(x => x.Value))
            {
                alteration(endpoint);
            }
        }

        /// <summary>
        /// Try to resolve the endpoint for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public TEndpoint For(string uri)
        {
            return For(new TransportUri(uri));
        }

        /// <summary>
        /// Try to resolve the endpoint for the given TransportUri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public TEndpoint For(TransportUri uri)
        {
            if (_endpoints.TryFind(uri, out var endpoint))
            {
                return endpoint;
            }

            if (uri.Protocol != _protocol) throw new ArgumentOutOfRangeException($"Invalid uri protocol '{uri.Protocol}', expected '{_protocol}'");

            var keys = uri.UriKeys();

            var invalids = keys.Where(x => !_validTransportUriKeys.Contains(x));
            if (invalids.Any())
            {
                throw new ArgumentOutOfRangeException($"Invalid {nameof(TransportUri)} value(s) for transport \"{uri.Protocol}\": {invalids.Join(", ")}");
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


        public TransportUri ReplyUri { get; set; }
    }
}
