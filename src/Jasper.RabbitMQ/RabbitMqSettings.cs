using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Transports;
using Jasper.Util;

// ReSharper disable InconsistentlySynchronizedField

namespace Jasper.RabbitMQ
{
    public class RabbitMqSettings : ExternalTransportSettings<RabbitMqEndpoint>
    {
        protected override RabbitMqEndpoint buildEndpoint(TransportUri uri, string connectionString)
        {
            return new RabbitMqEndpoint(uri, connectionString);
        }
    }
}
