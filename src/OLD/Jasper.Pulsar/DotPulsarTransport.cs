using System;
using System.Collections.Generic;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.DotPulsar.Internal;
using Jasper.Transports;

namespace Jasper.DotPulsar
{
    public static class Protocols
    {
        public static readonly string[] Pulsar = { "persistent", "non-persistent" };
    }

    public class DotPulsarTransport : TransportBase<DotPulsarEndpoint>
    {
        private readonly Dictionary<Uri, DotPulsarEndpoint> _endpoints;

        public DotPulsarTopicRouter Topics { get; } = new DotPulsarTopicRouter();
        public DotPulsarTransport() : base(DotPulsar.Protocols.Pulsar)
        {
            _endpoints = new Dictionary<Uri, DotPulsarEndpoint>();
        }

        public IPulsarClient PulsarClient { get; set; }

        protected override IEnumerable<DotPulsarEndpoint> endpoints() => _endpoints.Values;

        protected override DotPulsarEndpoint findEndpointByUri(Uri uri)
        {
            if (!_endpoints.ContainsKey(uri))
            {
                _endpoints.Add(uri, new DotPulsarEndpoint(uri)
                {
                    PulsarClient = PulsarClient
                });
            }

            return _endpoints[uri];
        }

        public DotPulsarEndpoint EndpointFor(ProducerOptions producerConifg) =>
            AddOrUpdateEndpoint(endpoint =>
            {
                endpoint.Topic = producerConifg.Topic;
                endpoint.ProducerOptions = producerConifg;
            });

        public DotPulsarEndpoint EndpointFor(ConsumerOptions consumerConifg) =>
            AddOrUpdateEndpoint(endpoint =>
            {
                endpoint.Topic = consumerConifg.Topic;
                endpoint.ConsumerOptions = consumerConifg;
            });

        DotPulsarEndpoint AddOrUpdateEndpoint(Action<DotPulsarEndpoint> configure)
        {
            var endpoint = new DotPulsarEndpoint
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
