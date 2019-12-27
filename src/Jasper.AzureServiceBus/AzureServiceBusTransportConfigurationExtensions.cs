using System;
using Baseline;
using Jasper.Configuration;

namespace Jasper.AzureServiceBus
{
    public static class AzureServiceBusTransportConfigurationExtensions
    {
        /// <summary>
        /// Quick access to the Azure Service Bus Transport within this application.
        /// This is for advanced usage
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        internal static AzureServiceBusTransport AsbTransport(this IEndpoints endpoints)
        {
            var transports = endpoints.As<TransportCollection>();
            var transport = transports.Get<AzureServiceBusTransport>();
            if (transport == null)
            {
                transport = new AzureServiceBusTransport();
                transports.Add(transport);
            }

            return transport;
        }

        /// <summary>
        /// Configure connection and authentication information about the Azure Service Bus usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigureAzureServiceBus(this IEndpoints endpoints, Action<IAzureServiceBusTransport> configure)
        {
            configure(endpoints.AsbTransport());
        }

        /// <summary>
        /// Configure connection and authentication information about the Azure Service Bus usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigureAzureServiceBus(this IEndpoints endpoints, string connectionString)
        {
            endpoints.ConfigureAzureServiceBus(asb => asb.ConnectionString = connectionString);
        }

        /// <summary>
        /// Listen for incoming messages at the designated Rabbit MQ queue by name
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="queueName">The name of the Rabbit MQ queue</param>
        /// <returns></returns>
        public static AzureServiceBusListenerConfiguration ListenToAzureServiceBusQueue(this IEndpoints endpoints, string queueName)
        {
            var endpoint = endpoints.AsbTransport().EndpointForQueue(queueName);
            endpoint.IsListener = true;
            return new AzureServiceBusListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Listen for incoming messages at the designated Rabbit MQ queue by name
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="queueName">The name of the Rabbit MQ queue</param>
        /// <returns></returns>
        public static AzureServiceBusListenerConfiguration ListenToAzureServiceBusTopic(this IEndpoints endpoints, string topicName, string subscriptionName)
        {
            var raw = new AzureServiceBusEndpoint{TopicName = topicName, SubscriptionName = subscriptionName}.Uri;
            var endpoint = endpoints.AsbTransport().GetOrCreateEndpoint(raw);
            endpoint.IsListener = true;
            return new AzureServiceBusListenerConfiguration((AzureServiceBusEndpoint) endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="routingKeyOrQueue">This is used as the routing key when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static AzureServiceBusSubscriberConfiguration ToAzureServiceBusQueue(this IPublishToExpression publishing, string queueName)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<AzureServiceBusTransport>();
            var endpoint = transport.EndpointForQueue(queueName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new AzureServiceBusSubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="routingKeyOrQueue">This is used as the routing key when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static AzureServiceBusSubscriberConfiguration ToAzureServiceBusTopic(this IPublishToExpression publishing, string topicName)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<AzureServiceBusTransport>();
            var endpoint = transport.EndpointForTopic(topicName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new AzureServiceBusSubscriberConfiguration(endpoint);
        }


    }
}
