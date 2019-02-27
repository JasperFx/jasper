using System;
using Jasper;
using Jasper.CommandLine;
using Microsoft.Extensions.Configuration;

namespace JasperService
{
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            // Add any necessary jasper options

            // Message publishing rules
            Publish.AllMessagesToUriValueInConfig("outbound");

            // Register listeners
            Transports.ListenForMessagesFromUriValueInConfig("inbound");

            // Explicitly registers the Azure Service Bus transport
            Include<Jasper.AzureServiceBus.AzureServiceBusTransportExtension>();
        }
    }

}