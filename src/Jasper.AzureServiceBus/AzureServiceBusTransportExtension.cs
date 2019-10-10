using Jasper;
using Jasper.AzureServiceBus;
using Jasper.AzureServiceBus.Internal;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.Settings;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(AzureServiceBusTransportExtension))]

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<AzureServiceBusOptions>();
            registry.Services.AddTransient<ITransport, AzureServiceBusTransport>();
        }
    }
}
