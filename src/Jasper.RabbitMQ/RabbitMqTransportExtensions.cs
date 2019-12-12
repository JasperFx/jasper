using System;
using Baseline;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqTransportExtensions
    {
        private static RabbitMqTransport RabbitMqTransport(this IEndpoints endpoints)
        {
            return endpoints.As<TransportCollection>().Get<RabbitMqTransport>();
        }

        public static void RabbitMqConnection(this IEndpoints endpoints, Uri ampqUri)
        {
            var rabbit = endpoints.RabbitMqTransport();
            rabbit.ConnectionFactory.Uri = ampqUri;
        }

        public static void RabbitMqConnection(this IEndpoints endpoints, Action<ConnectionFactory> configure)
        {
            var rabbit = endpoints.RabbitMqTransport();
            configure(rabbit.ConnectionFactory);
        }
    }
}
