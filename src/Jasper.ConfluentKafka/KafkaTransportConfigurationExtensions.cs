using System;
using Baseline;
using Confluent.Kafka;
using Jasper.Configuration;

namespace Jasper.ConfluentKafka
{
    public static class KafkaTransportConfigurationExtensions
    {/// <summary>
        /// Quick access to the kafka Transport within this application.
        /// This is for advanced usage
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        internal static KafkaTransport KafkaTransport(this IEndpoints endpoints)
        {
            var transports = endpoints.As<TransportCollection>();

            var transport = transports.Get<KafkaTransport>();

            if (transport == null)
            {
                transport = new KafkaTransport();
                transports.Add(transport);
            }

            transports.Subscribers.Fill(transport.Topics);

            return transport;
        }
        /// <summary>
        /// Configure connection and authentication information about the Azure Service Bus usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigureKafka(this IEndpoints endpoints, Action<KafkaTransport> configure)
        {
            var transport = endpoints.KafkaTransport();
            endpoints.As<TransportCollection>().Subscribers.Fill(transport.Topics);
            configure(transport);
        }

        /// <summary>
        /// Configure connection and authentication information about the Azure Service Bus usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigureKafka(this IEndpoints endpoints)
        {
            endpoints.ConfigureKafka(_ =>
            {
                
            });
        }

        /// <summary>
        /// Listen for incoming messages at the designated Rabbit MQ queue by name
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="queueName">The name of the Rabbit MQ queue</param>
        /// <returns></returns>
        //public static KafkaListenerConfiguration ListenToKafkaTopic(this IEndpoints endpoints, string topicName, string subscriptionName)
        //{
        //    var raw = new KafkaEndpoint { TopicName = topicName, SubscriptionName = subscriptionName }.Uri;
        //    var endpoint = endpoints.KafkaTransport().GetOrCreateEndpoint(raw);
        //    endpoint.IsListener = true;
        //    return new KafkaListenerConfiguration((KafkaEndpoint)endpoint);
        //}

        /// <summary>
        /// Publish matching messages to Rabbit MQ using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="topicName">This is used as the topic name when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static KafkaSubscriberConfiguration ToKafkaTopic<TKey, TVal>(this IPublishToExpression publishing, string topicName, ProducerConfig producerConfig)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<KafkaTransport>();
            var endpoint = transport.EndpointForTopic<TKey, TVal>(topicName, producerConfig);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new KafkaSubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Azure Service Bus using the topic name derived from the message and
        /// </summary>
        /// <param name="publishing"></param>
        /// <returns></returns>
        public static TopicRouterConfiguration<KafkaSubscriberConfiguration> ToKafkaTopics(this IPublishToExpression publishing)
        {
            var transports = publishing.As<PublishingExpression>().Parent;

            var router = transports.KafkaTransport().Topics;

            publishing.ViaRouter(router);

            return new TopicRouterConfiguration<KafkaSubscriberConfiguration>(router, transports);
        }
    }
}
