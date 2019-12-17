using System;
using Baseline;
using Jasper.Configuration;
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

        public static RabbitMqListenerConfiguration ListenAtRabbitQueue(this IEndpoints endpoints, string queueName)
        {
            throw new NotImplementedException();
        }

        public static RabbitMqSubscriberConfiguration ToRabbit(string routingKey, string exchangeName = "")
        {
            throw new NotImplementedException();
        }
    }

    public class RabbitMqSubscriberConfiguration : SubscriberConfiguration<RabbitMqSubscriberConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqSubscriberConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        public RabbitMqSubscriberConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        public RabbitMqSubscriberConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            _endpoint.Protocol = protocol;
            return this;
        }
    }

    public class RabbitMqListenerConfiguration : ListenerConfiguration<RabbitMqListenerConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqListenerConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        public RabbitMqListenerConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        public RabbitMqListenerConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            endpoint.Protocol = protocol;
            return this;
        }
    }


}
