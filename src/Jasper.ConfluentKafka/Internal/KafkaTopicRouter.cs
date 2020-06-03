using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime.Routing;

namespace Jasper.ConfluentKafka.Internal
{
    public class KafkaTopicRouter : TopicRouter<KafkaSubscriberConfiguration>
    {
        public override Uri BuildUriForTopic(string topicName)
        {
            // TODO -- this probably shouldn't be durable by default, but
            // that's how it was coded before
            var endpoint = new KafkaEndpoint
            {
                Mode = EndpointMode.Durable,
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
