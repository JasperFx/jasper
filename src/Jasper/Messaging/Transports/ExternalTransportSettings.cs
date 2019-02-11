using System;
using System.Collections.Generic;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class ExternalTransportSettings<TEndpoint> where TEndpoint : class
    {
        private ImHashMap<string, TEndpoint> _endpoints = ImHashMap<string,TEndpoint>.Empty;

        private readonly object _locker = new object();

        public Dictionary<string, string> Connections { get; set; } = new Dictionary<string, string>();

        protected abstract TEndpoint buildEndpoint(string name, string connectionString);

        public void ConfigureEndpoint(string connectionName, Action<TEndpoint> alteration)
        {
            var endpoint = For(connectionName);
            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(connectionName), $"Unknown connection named '{connectionName}'");

            alteration(endpoint);
        }

        public TEndpoint For(string connectionName)
        {
            if (_endpoints.TryFind(connectionName, out var endpoint))
            {
                return endpoint;
            }

            lock (_locker)
            {
                if (_endpoints.TryFind(connectionName, out endpoint))
                {
                    return endpoint;
                }

                if (!Connections.ContainsKey(connectionName)) return null;

                endpoint = buildEndpoint(connectionName, Connections[connectionName]);
                _endpoints = _endpoints.AddOrUpdate(connectionName, endpoint);

                return endpoint;

            }
        }

        // TODO -- use the service name if you can?
        public Uri ReplyUri { get; set; }
    }
}