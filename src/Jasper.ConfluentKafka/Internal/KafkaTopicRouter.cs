using System;
using System.Collections.Generic;
using Baseline;
using Confluent.Kafka;
using Jasper.Configuration;
using Jasper.Runtime.Routing;

namespace Jasper.ConfluentKafka.Internal
{
    public class KafkaTopicRouter : TopicRouter<KafkaSubscriberConfiguration>
    {
        //Dictionary<Uri, ProducerConfig> producerConfigs;
        //public KafkaTopicRouter(Dictionary<Uri, ProducerConfig> producerConfigs)
        //{
        //}

        public override Uri BuildUriForTopic(string topicName)
        {
            var endpoint = new KafkaEndpoint
            {
                IsDurable = true,
                TopicName = topicName
            };

            return endpoint.Uri;
        }

        public override KafkaSubscriberConfiguration FindConfigurationForTopic(string topicName,
            IEndpoints endpoints)
        {
            var uri = BuildUriForTopic(topicName);
            var endpoint = endpoints.As<TransportCollection>().GetOrCreateEndpoint(uri);

            return new KafkaSubscriberConfiguration((KafkaEndpoint) endpoint);
        }

    }

}
