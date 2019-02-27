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

            // Helps out the auto-discovery of Jasper extensions
            Include<Jasper.RabbitMQ.RabbitMqTransportExtension>();
        }
    }

}