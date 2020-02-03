using System;
using Baseline;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqTransportExtensions
    {
        /// <summary>
        /// Quick access to the Rabbit MQ Transport within this application.
        /// This is for advanced usage
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        internal static RabbitMqTransport RabbitMqTransport(this IEndpoints endpoints)
        {
            var transports = endpoints.As<TransportCollection>();
            var transport = transports.Get<RabbitMqTransport>();
            if (transport == null)
            {
                transport = new RabbitMqTransport();
                transports.Add(transport);
            }

            return transport;
        }

        /// <summary>
        /// Configure connection and authentication information about the Rabbit MQ usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigureRabbitMq(this IEndpoints endpoints, Action<IRabbitMqTransport> configure)
        {
            configure(endpoints.RabbitMqTransport());
        }

        /// <summary>
        /// Listen for incoming messages at the designated Rabbit MQ queue by name
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="queueName">The name of the Rabbit MQ queue</param>
        /// <returns></returns>
        public static RabbitMqListenerConfiguration ListenToRabbitQueue(this IEndpoints endpoints, string queueName)
        {
            var endpoint = endpoints.RabbitMqTransport().EndpointForQueue(queueName);
            endpoint.IsListener = true;
            return new RabbitMqListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="routingKeyOrQueue">This is used as the routing key when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static RabbitMqSubscriberConfiguration ToRabbit(this IPublishToExpression publishing, string routingKeyOrQueue, string exchangeName = "")
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            var endpoint =  transport.EndpointFor(routingKeyOrQueue, exchangeName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new RabbitMqSubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ to the designated exchange. This is
        /// appropriate for "fanout" exchanges where Rabbit MQ ignores the routing key
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="exchangeName">The Rabbit MQ exchange name</param>
        /// <returns></returns>
        public static RabbitMqSubscriberConfiguration ToRabbitExchange(this IPublishToExpression publishing, string exchangeName)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            var endpoint =  transport.EndpointForExchange(exchangeName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new RabbitMqSubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ to the designated exchange by the topic name for the message
        /// type or the designated topic name in the Envelope. This is *only* usable
        /// for "topic" exchanges
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="exchangeName">The Rabbit MQ exchange name</param>
        /// <returns></returns>
        public static TopicRouterConfiguration<RabbitMqSubscriberConfiguration> ToRabbitTopics(this IPublishToExpression publishing, string exchangeName)
        {
            var transports = publishing.As<PublishingExpression>().Parent;

            var router = new RabbitMqTopicRouter(exchangeName);

            publishing.ViaRouter(router);

            return new TopicRouterConfiguration<RabbitMqSubscriberConfiguration>(router, transports);
        }
    }
}
