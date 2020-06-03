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
            // TODO -- evaluate whether or not we really want the default to be durable
            var endpoint = new PulsarEndpoint
            {
                Mode = EndpointMode.Durable,
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
