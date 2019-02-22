using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Transports;
using Jasper.RabbitMQ.Internal;
using Jasper.Util;

// ReSharper disable InconsistentlySynchronizedField

namespace Jasper.RabbitMQ
{
    public class RabbitMqOptions : ExternalTransportSettings<RabbitMqEndpoint>
    {
        public RabbitMqOptions() : base("rabbitmq",TransportConstants.Queue, TransportConstants.Topic, TransportConstants.Routing )
        {
        }

        protected override RabbitMqEndpoint buildEndpoint(TransportUri uri, string connectionString)
        {
            return new RabbitMqEndpoint(uri, connectionString);
        }
    }
}
