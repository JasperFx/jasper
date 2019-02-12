using Jasper;
using Jasper.AzureServiceBus.Internal;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(AzureServiceBusTransportExtension))]

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<AzureServiceBusSettings>();
            registry.Services.AddTransient<ITransport, AzureServiceBusTransport>();
        }
    }
}
