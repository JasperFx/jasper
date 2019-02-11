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

        public Endpoint ForEndpoint(string uriString)
        {
            return ForEndpoint(uriString.ToUri());
        }

        public Endpoint ForEndpoint(Uri uri)
        {
            if (_endpoints.ContainsKey(uri)) return _endpoints[uri];

            lock (_locker)
            {
                if (_endpoints.ContainsKey(uri)) return _endpoints[uri];


                var endpoint = _endpoints.Values.FirstOrDefault(x => x.ToFullUri() == uri);


                if (endpoint == null)
                {
                    if (uri.Segments.Length > 2)
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
                            return null;
                        }
                    }
                }

                endpoint.Broker = BrokerFor(endpoint.BrokerUri);

                _endpoints[endpoint.Uri] = endpoint;
                _endpoints[endpoint.ToFullUri()] = endpoint;

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

        /// <summary>
        /// Designate a Uri to be the listener for incoming replies to this application
        /// </summary>
        public Uri ReplyUri { get; set; } = new Uri("rabbitmq://replies");


        public Dictionary<string, string> Connections { get; set; } = new Dictionary<string, string>();
    }
}
