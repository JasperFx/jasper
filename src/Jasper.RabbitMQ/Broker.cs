using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class Broker
    {
        public Broker(Uri uri)
        {
            if (uri.Scheme != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");
            Uri = uri;

            ConnectionFactory.Port = uri.IsDefaultPort ? 5672 : uri.Port;
            ConnectionFactory.HostName = uri.Host;
        }

        public IConnection OpenConnection()
        {
            return AmqpTcpEndpoints.Any()
                ? ConnectionFactory.CreateConnection(AmqpTcpEndpoints)
                : ConnectionFactory.CreateConnection();
        }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();

        public Uri Uri { get; }

    }
}
