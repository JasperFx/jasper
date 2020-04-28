using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Internal;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public static class Protocols
    {
        public const string Kafka = "kafka";

    }

    public class KafkaTransport : TransportBase<KafkaEndpoint>
    {
        private readonly Dictionary<Uri, KafkaEndpoint> _endpoints;

        public KafkaTopicRouter Topics { get; } = new KafkaTopicRouter();
        public KafkaTransport() : base(Protocols.Kafka)
        {
            _endpoints = new Dictionary<Uri, KafkaEndpoint>();
        }

        protected override IEnumerable<KafkaEndpoint> endpoints() => _endpoints.Values;

        protected override KafkaEndpoint findEndpointByUri(Uri uri) => _endpoints[uri];

        public KafkaEndpoint EndpointForTopic(string topicName, ProducerConfig producerConifg) =>
            AddOrUpdateEndpoint(topicName, endpoint => endpoint.ProducerConfig = producerConifg);

        public KafkaEndpoint EndpointForTopic(string topicName, ConsumerConfig consumerConifg) =>
            AddOrUpdateEndpoint(topicName, endpoint => endpoint.ConsumerConfig = consumerConifg);

        KafkaEndpoint AddOrUpdateEndpoint(string topicName, Action<KafkaEndpoint> configure)
        {
            var endpoint = new KafkaEndpoint
            {
                TopicName = topicName
            };

            if (_endpoints.ContainsKey(endpoint.Uri))
            {
                endpoint = _endpoints[endpoint.Uri];
                configure(endpoint);
            }
            else
            {
                configure(endpoint);
                _endpoints.Add(endpoint.Uri, endpoint);
            }

            return endpoint;
        }

    }
}
