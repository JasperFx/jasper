using System;
using Baseline;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public interface IRabbitMqTransportExpression
    {
        // TODO -- both options with environment = Development
        IRabbitMqTransportExpression AutoProvision();
        IRabbitMqTransportExpression AutoPurgeOnStartup();

        /// <summary>
        /// Declare a binding from a Rabbit Mq exchange to a Rabbit MQ queue
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="configure">Optional configuration of the Rabbit MQ exchange</param>
        /// <returns></returns>
        IBindingExpression BindExchange(string exchangeName, Action<RabbitMqExchange>? configure = null);

        /// <summary>
        /// Declare a binding from a Rabbit Mq exchange to a Rabbit MQ queue
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        IBindingExpression BindExchange(string exchangeName, ExchangeType exchangeType);


        /// <summary>
        /// Declare that a queue should be created with the supplied name and optional configuration
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="configure"></param>
        IRabbitMqTransportExpression DeclareQueue(string queueName, Action<RabbitMqQueue>? configure = null);

        /// <summary>
        /// Declare a new exchange. The default exchange type is "fan out"
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="configure"></param>
        IRabbitMqTransportExpression DeclareExchange(string exchangeName, Action<RabbitMqExchange>? configure = null);

        /// <summary>
        /// Declare a new exchange with the specified exchange type
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="configure"></param>
        IRabbitMqTransportExpression DeclareExchange(string exchangeName, ExchangeType exchangeType, bool isDurable = true, bool autoDelete = false);

    }

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
            var transports = endpoints.As<JasperOptions>();
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
        public static IRabbitMqTransportExpression UseRabbitMq(this IEndpoints endpoints, Action<ConnectionFactory> configure)
        {
            var transport = endpoints.RabbitMqTransport();
            configure(transport.ConnectionFactory);

            return transport;
        }

        /// <summary>
        /// Connect to Rabbit MQ on the local machine with all the default
        /// Rabbit MQ client options
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="rabbitMqUri">Rabbit MQ Uri that designates the connection information. See https://www.rabbitmq.com/uri-spec.html</param>
        public static IRabbitMqTransportExpression UseRabbitMq(this IEndpoints endpoints, Uri rabbitMqUri)
        {
            return endpoints.UseRabbitMq(factory => factory.Uri = rabbitMqUri);
        }

        /// <summary>
        /// Connect to Rabbit MQ on the local machine with all the default
        /// Rabbit MQ client options
        /// </summary>
        /// <param name="endpoints"></param>
        public static IRabbitMqTransportExpression UseRabbitMq(this IEndpoints endpoints)
        {
            return endpoints.UseRabbitMq(t => {});
        }

        /// <summary>
        /// Listen for incoming messages at the designated Rabbit MQ queue by name
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="queueName">The name of the Rabbit MQ queue</param>
        /// <param name="configure">Optional configuration for this Rabbit Mq queue if being initialized by Jasper
        ///
        /// <returns></returns>
        public static RabbitMqListenerConfiguration ListenToRabbitQueue(this IEndpoints endpoints, string queueName, Action<RabbitMqQueue>? configure = null)
        {
            var transport = endpoints.RabbitMqTransport();
            transport.DeclareQueue(queueName, configure);
            var endpoint = transport.EndpointForQueue(queueName);
            endpoint.IsListener = true;

            return new RabbitMqListenerConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages to Rabbit MQ using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="routingKeyOrQueueName">This is used as the routing key when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <param name="exchangeName">Optional, you only need to supply this if you are using a non-default exchange</param>
        /// <returns></returns>
        public static RabbitMqSubscriberConfiguration ToRabbit(this IPublishToExpression publishing, string routingKeyOrQueueName, string exchangeName = "")
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            var endpoint =  transport.EndpointFor(routingKeyOrQueueName, exchangeName);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new RabbitMqSubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Publish matching messages straight to a Rabbit MQ queue using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="routingKeyOrQueue">This is used as the routing key when publishing. Can be either a binding key or a queue name or a static topic name if the exchange is topic-based</param>
        /// <returns></returns>
        public static RabbitMqSubscriberConfiguration ToRabbitQueue(this IPublishToExpression publishing, string queueName, Action<RabbitMqQueue>? configure = null)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();
            transport.DeclareQueue(queueName, configure);

            var endpoint =  transport.EndpointForQueue(queueName);

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
        /// <param name="configure">Optional configuration of this exchange if Jasper is doing the initialization in Rabbit MQ</param>
        /// <returns></returns>
        public static RabbitMqSubscriberConfiguration ToRabbitExchange(this IPublishToExpression publishing, string exchangeName, Action<RabbitMqExchange>? configure = null)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<RabbitMqTransport>();

            transport.DeclareExchange(exchangeName, configure);

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
