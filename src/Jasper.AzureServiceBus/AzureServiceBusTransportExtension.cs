using Jasper;
using Jasper.AzureServiceBus;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(AzureServiceBusTransportExtension))]

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusTransportExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.Settings.Require<AzureServiceBusSettings>();
            registry.Services.AddTransient<ITransport, AzureServiceBusTransport>();
        }
    }
}
