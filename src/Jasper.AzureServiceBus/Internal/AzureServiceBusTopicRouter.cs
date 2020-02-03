using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Runtime.Routing;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusTopicRouter : TopicRouter<AzureServiceBusSubscriberConfiguration>
    {
        public AzureServiceBusTopicRouter()
        {
        }

        public override Uri BuildUriForTopic(string topicName)
        {
            var endpoint = new AzureServiceBusEndpoint
            {
                IsDurable = IsDurable,
                TopicName = topicName
            };

            return endpoint.Uri;
        }

        public override AzureServiceBusSubscriberConfiguration FindConfigurationForTopic(string topicName,
            IEndpoints endpoints)
        {
            var uri = BuildUriForTopic(topicName);
            var endpoint = endpoints.As<TransportCollection>().GetOrCreateEndpoint(uri);

            return new AzureServiceBusSubscriberConfiguration((AzureServiceBusEndpoint) endpoint);
        }

    }

}
