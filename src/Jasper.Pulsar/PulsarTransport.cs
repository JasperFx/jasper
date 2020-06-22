using System;
using System.Collections.Generic;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Pulsar.Internal;
using Jasper.Transports;

namespace Jasper.Pulsar
{
    public static class Protocols
    {
        public static readonly string[] Pulsar = { "persistent", "non-persistent" };
    }

    public class PulsarTransport : TransportBase<PulsarEndpoint>
    {
        private readonly Dictionary<Uri, PulsarEndpoint> _endpoints;

        public PulsarTopicRouter Topics { get; } = new PulsarTopicRouter();
        public PulsarTransport() : base(Pulsar.Protocols.Pulsar)
        {
            _endpoints = new Dictionary<Uri, PulsarEndpoint>();
        }

        public IPulsarClient PulsarClient { get; set; }

        protected override IEnumerable<PulsarEndpoint> endpoints() => _endpoints.Values;

        protected override PulsarEndpoint findEndpointByUri(Uri uri)
        {
            if (!_endpoints.ContainsKey(uri))
            {
                _endpoints.Add(uri, new PulsarEndpoint(uri)
                {
                    PulsarClient = PulsarClient
                });
            }

            return _endpoints[uri];
        }

        public PulsarEndpoint EndpointFor(ProducerOptions producerConifg) =>
            AddOrUpdateEndpoint(endpoint =>
            {
                endpoint.Topic = producerConifg.Topic;
                endpoint.ProducerOptions = producerConifg;
            });

        public PulsarEndpoint EndpointFor(ConsumerOptions consumerConifg) =>
            AddOrUpdateEndpoint(endpoint =>
            {
                endpoint.Topic = consumerConifg.Topic;
                endpoint.ConsumerOptions = consumerConifg;
            });

        PulsarEndpoint AddOrUpdateEndpoint(Action<PulsarEndpoint> configure)
        {
            var endpoint = new PulsarEndpoint
            {
                PulsarClient = PulsarClient
            };

            configure(endpoint);

            if (_endpoints.ContainsKey(endpoint.Uri))
            {
                endpoint = _endpoints[endpoint.Uri];
                configure(endpoint);
            }
            else
            {
                _endpoints.Add(endpoint.Uri, endpoint);
            }

            return endpoint;
        }

    }
}
