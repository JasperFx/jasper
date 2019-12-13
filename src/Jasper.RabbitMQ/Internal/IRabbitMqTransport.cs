using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public interface IRabbitMqTransport
    {
        bool AutoProvision { get; set; }
        ConnectionFactory ConnectionFactory { get; }
        IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; }
        void DeclareBinding(Binding binding);
        void DeclareExchange(string exchangeName, Action<RabbitMqExchange> configure = null);
        void DeclareQueue(string queueName, Action<RabbitMqQueue> configure = null);
    }
}