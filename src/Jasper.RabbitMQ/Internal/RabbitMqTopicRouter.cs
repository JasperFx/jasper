using System;
using Jasper.Configuration;
using Jasper.Runtime.Routing;
using LamarCodeGeneration.Util;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqTopicRouter : TopicRouter<RabbitMqSubscriberConfiguration>
    {
        public RabbitMqTopicRouter(string exchangeName)
        {
            ExchangeName = exchangeName;
        }

        public string ExchangeName { get; }


        public override Uri BuildUriForTopic(string topicName)
        {
            var endpoint = new RabbitMqEndpoint(new RabbitMqTransport())
            {
                ExchangeName = ExchangeName,
                Mode = Mode,
                RoutingKey = topicName
            };

            return endpoint.Uri;
        }

        public override RabbitMqSubscriberConfiguration FindConfigurationForTopic(string topicName,
            IEndpoints endpoints)
        {
            var uri = BuildUriForTopic(topicName);
            var endpoint = endpoints.As<JasperOptions>().GetOrCreateEndpoint(uri);

            return new RabbitMqSubscriberConfiguration((RabbitMqEndpoint)endpoint);
        }
    }
}
