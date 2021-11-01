using System;
using Baseline;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Internal;
using Jasper.Configuration;

namespace Jasper.DotPulsar
{
    public static class DotPulsarTransportConfigurationExtensions
    {/// <summary>
        /// Quick access to the pulsar Transport within this application.
        /// This is for advanced usage
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        internal static DotPulsarTransport PulsarTransport(this IEndpoints endpoints)
        {
            var transports = endpoints.As<TransportCollection>();

            var transport = transports.Get<DotPulsarTransport>();

            if (transport == null)
            {
                transport = new DotPulsarTransport();
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
        public static void ConfigurePulsar(this IEndpoints endpoints, Action<DotPulsarTransport> configure)
        {
            var transport = endpoints.PulsarTransport();
            endpoints.As<TransportCollection>().Subscribers.Fill(transport.Topics);
            configure(transport);
        }

        /// <summary>
        /// Configure connection and authentication information about the Azure Service Bus usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigurePulsar(this IEndpoints endpoints, IPulsarClient client)
        {
            endpoints.ConfigurePulsar(_ => { _.PulsarClient = client; });
        }


        public static void ConfigurePulsar(this IEndpoints endpoints, string pulsarCluster)
        {
            endpoints.ConfigurePulsar(_ =>
            {
                _.PulsarClient = new PulsarClientBuilder().ServiceUrl(new Uri(pulsarCluster)).Build();
            });
        }

        public static void ConfigurePulsar(this IEndpoints endpoints, Uri pulsarCluster)
        {
            endpoints.ConfigurePulsar(_ =>
            {
                _.PulsarClient = new PulsarClientBuilder().ServiceUrl(pulsarCluster).Build();
            });
        }

        public static void ConfigurePulsar(this IEndpoints endpoints, IPulsarClientBuilder pulsarClientBuilder)
        {
            endpoints.ConfigurePulsar(_ =>
            {
                _.PulsarClient = pulsarClientBuilder.Build();
            });
        }

        /// <summary>
        /// Listen for incoming messages at the designated Pulsar Topic by name
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="topicName"></param>
        /// <param name="consumerConfig"></param>
        /// <returns></returns>
        public static DotPulsarListenerConfiguration ListenToPulsarTopic(this IEndpoints endpoints, string subscription, string topicName) =>
            ListenToPulsarTopic(endpoints, new ConsumerOptions(subscription, topicName));
        
        public static DotPulsarListenerConfiguration ListenToPulsarTopic(this IEndpoints endpoints, ConsumerOptions consumerConfig)
        {
            var endpoint = endpoints.PulsarTransport().EndpointFor(consumerConfig);
            endpoint.IsListener = true;
            return new DotPulsarListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Pulsar Topic using provided Producer Configuration
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="topicName">This is used as the topic name when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static DotPulsarSubscriberConfiguration ToPulsarTopic(this IPublishToExpression publishing, string topicName) => ToPulsarTopic(publishing, new ProducerOptions(topicName));

        public static DotPulsarSubscriberConfiguration ToPulsarTopic(this IPublishToExpression publishing, ProducerOptions producerOptions)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<DotPulsarTransport>();
            var endpoint = transport.EndpointFor(producerOptions);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new DotPulsarSubscriberConfiguration(endpoint);
        }

    }
}
