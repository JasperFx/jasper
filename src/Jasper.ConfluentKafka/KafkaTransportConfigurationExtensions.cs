using System;
using Baseline;
using Confluent.Kafka;
using Jasper.Configuration;
using Jasper.Kafka;

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
        /// Listen for incoming messages at the designated Kafka Topic by name
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="topicName"></param>
        /// <param name="consumerConfig"></param>
        /// <returns></returns>
        public static KafkaListenerConfiguration ListenToKafkaTopic<TKey, TVal>(this IEndpoints endpoints, string topicName, ConsumerConfig consumerConfig)
        {
            var endpoint = endpoints.KafkaTransport().EndpointForTopic<TKey, TVal>(topicName, consumerConfig);
            endpoint.IsListener = true;
            return new KafkaListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Listen for incoming messages at the designated Kafka Topic by name
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="topicName"></param>
        /// <param name="consumerConfig"></param>
        /// <param name="keyDeserializer"></param>
        /// <param name="valueDeserializer"></param>
        /// <returns></returns>
        public static KafkaListenerConfiguration ListenToKafkaTopic<TKey, TVal>(this IEndpoints endpoints, string topicName, ConsumerConfig consumerConfig,
            IDeserializer<TKey> keyDeserializer, IDeserializer<TVal> valueDeserializer)
        {
            var endpoint = endpoints.KafkaTransport().EndpointForTopic<TKey, TVal>(topicName, consumerConfig, keyDeserializer, valueDeserializer);
            endpoint.IsListener = true;
            return new KafkaListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Kafka Topic using provided Producer Configuration
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
        /// Publish matching messages to Kafka Topic using provided Producer Configuration and Serializers
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="publishing"></param>
        /// <param name="topicName"></param>
        /// <param name="producerConfig"></param>
        /// <param name="keySerializer"></param>
        /// <param name="valueSerializer"></param>
        /// <returns></returns>
        public static KafkaSubscriberConfiguration ToKafkaTopic<TKey, TVal>(this IPublishToExpression publishing, string topicName, ProducerConfig producerConfig, ISerializer<TKey> keySerializer, ISerializer<TVal> valueSerializer)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<KafkaTransport>();
            var endpoint = transport.EndpointForTopic(topicName, producerConfig, keySerializer, valueSerializer);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new KafkaSubscriberConfiguration(endpoint);
        }
    }
}
