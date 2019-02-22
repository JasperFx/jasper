using System;
using System.Collections.Generic;
using Jasper.AzureServiceBus.Internal;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusOptions : ExternalTransportSettings<AzureServiceBusEndpoint>
    {
        public AzureServiceBusOptions() : base(AzureServiceBusTransport.ProtocolName,TransportConstants.Queue, TransportConstants.Subscription, TransportConstants.Topic)
        {
        }

        protected override AzureServiceBusEndpoint buildEndpoint(TransportUri uri, string connectionString)
        {
            return new AzureServiceBusEndpoint(uri, connectionString);
        }
    }
}
