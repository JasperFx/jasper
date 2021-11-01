using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime.Routing;

namespace Jasper.DotPulsar.Internal
{
    public class DotPulsarTopicRouter : TopicRouter<DotPulsarSubscriberConfiguration>
    {
        public override Uri BuildUriForTopic(string topic)
        {
            // TODO -- evaluate whether or not we really want the default to be durable
            var endpoint = new DotPulsarEndpoint
            {
                Mode = EndpointMode.Durable,
                Topic = topic
            };

            return endpoint.Uri;
        }

        public override DotPulsarSubscriberConfiguration FindConfigurationForTopic(string topicName,
            IEndpoints endpoints)
        {
            Uri uri = BuildUriForTopic(topicName);
            Endpoint endpoint = endpoints.As<TransportCollection>().GetOrCreateEndpoint(uri);

            return new DotPulsarSubscriberConfiguration((DotPulsarEndpoint) endpoint);
        }
    }
}
