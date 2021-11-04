using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime.Routing;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusTopicRouter : TopicRouter<AzureServiceBusSubscriberConfiguration>
    {
        public override Uri BuildUriForTopic(string topicName)
        {
            var endpoint = new AzureServiceBusEndpoint
            {
                Mode = Mode,
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
