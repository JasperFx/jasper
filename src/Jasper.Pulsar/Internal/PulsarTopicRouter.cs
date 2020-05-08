using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime.Routing;

namespace Jasper.Pulsar.Internal
{
    public class PulsarTopicRouter : TopicRouter<PulsarSubscriberConfiguration>
    {
        public override Uri BuildUriForTopic(string topic)
        {
            var endpoint = new PulsarEndpoint
            {
                IsDurable = true,
                Topic = topic
            };

            return endpoint.Uri;
        }

        public override PulsarSubscriberConfiguration FindConfigurationForTopic(string topicName,
            IEndpoints endpoints)
        {
            Uri uri = BuildUriForTopic(topicName);
            Endpoint endpoint = endpoints.As<TransportCollection>().GetOrCreateEndpoint(uri);

            return new PulsarSubscriberConfiguration((PulsarEndpoint) endpoint);
        }
    }
}
