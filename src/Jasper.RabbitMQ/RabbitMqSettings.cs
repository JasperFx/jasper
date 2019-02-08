using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Jasper.Util;

// ReSharper disable InconsistentlySynchronizedField

namespace Jasper.RabbitMQ
{
    public class RabbitMqSettings
    {
        private readonly ConcurrentDictionary<Uri, Endpoint> _endpoints =
            new ConcurrentDictionary<Uri, Endpoint>();
        
        private readonly ConcurrentDictionary<Uri, Broker> _brokers = new ConcurrentDictionary<Uri, Broker>();

        private readonly object _locker = new object();

        public Endpoint For(string uriString)
        {
            return For(uriString.ToUri());
        }

        public Endpoint For(Uri uri)
        {
            if (_endpoints.ContainsKey(uri)) return _endpoints[uri];

            lock (_locker)
            {
                if (_endpoints.ContainsKey(uri)) return _endpoints[uri];


                Endpoint endpoint;

                if (uri.Segments.Any())
                {
                    endpoint = new Endpoint(uri);
                }
                else
                {
                    if (Connections.TryGetValue(uri.Host, out var connectionString))
                    {
                        endpoint = new Endpoint(uri.Host, connectionString);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown connection key '{uri.Host}' for the Rabbit MQ uri {uri}");
                    }
                }

                endpoint.Broker = BrokerFor(endpoint.BrokerUri);

                _endpoints[endpoint.Uri] = endpoint;

                return endpoint;
            }
        }

        public Broker BrokerFor(Uri brokerUri)
        {
            if (_brokers.TryGetValue(brokerUri, out var broker))
            {
                return broker;
            }
            
            broker = new Broker(brokerUri);
            _brokers[brokerUri] = broker;

            return broker;
        }


        public Dictionary<string, string> Connections { get; set; } = new Dictionary<string, string>();
    }
}
