using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public interface IRabbitMqTransport
    {
        [Obsolete("replace with Oakton equivalents")]
        bool AutoProvision { get; set; }

        /// <summary>
        /// For test automation purposes, setting this to true will direct Jasper
        /// to purge messages out of all configured queues on startup
        /// </summary>
        [Obsolete("replace with Oakton equivalents")]
        bool AutoPurgeOnStartup { get; set; }
        ConnectionFactory ConnectionFactory { get; }
        IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; }
        void DeclareBinding(Binding binding);
        void DeclareExchange(string exchangeName, Action<RabbitMqExchange> configure = null);
        void DeclareQueue(string queueName, Action<RabbitMqQueue> configure = null);
    }
}
