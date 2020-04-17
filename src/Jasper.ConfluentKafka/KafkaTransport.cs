using System;
using System.Collections.Generic;
using Baseline;
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
        private readonly LightweightCache<Uri, KafkaEndpoint> _endpoints;

        public KafkaTopicRouter Topics { get; } = new KafkaTopicRouter();

        public KafkaTransport() : base(Protocols.Kafka)
        {
        }

        protected override IEnumerable<KafkaEndpoint> endpoints() => _endpoints;

        protected override KafkaEndpoint findEndpointByUri(Uri uri) => _endpoints[uri];

        public KafkaEndpoint EndpointForTopic(string topicName)
        {
            var uri = new KafkaEndpoint { TopicName = topicName }.Uri;
            return _endpoints[uri];
        }
    }
}
