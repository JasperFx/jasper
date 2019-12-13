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
    }
}
