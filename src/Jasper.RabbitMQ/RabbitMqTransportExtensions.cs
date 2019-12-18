using System;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqTransportExtensions
    {
        public static RabbitMqTransport RabbitMqTransport(this IEndpoints endpoints)
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

        public static void ConfigureRabbitMq(this IEndpoints endpoints, Action<IRabbitMqTransport> configure)
        {
            configure(endpoints.RabbitMqTransport());
        }

        public static RabbitMqListenerConfiguration ListenToRabbitQueue(this IEndpoints endpoints, string queueName)
        {
            var endpoint = endpoints.RabbitMqTransport().EndpointForQueue(queueName);
            endpoint.IsListener = true;
            return new RabbitMqListenerConfiguration(endpoint);
        }

        public static RabbitMqSubscriberConfiguration ToRabbit(this IPublishToExpression publishing, string routingKeyOrQueue, string exchangeName = "")
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            var endpoint =  transport.EndpointFor(routingKeyOrQueue, exchangeName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new RabbitMqSubscriberConfiguration(endpoint);
        }

        public static RabbitMqSubscriberConfiguration ToRabbitExchange(this IPublishToExpression publishing, string exchangeName)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            var endpoint =  transport.EndpointForExchange(exchangeName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new RabbitMqSubscriberConfiguration(endpoint);
        }
    }
}
