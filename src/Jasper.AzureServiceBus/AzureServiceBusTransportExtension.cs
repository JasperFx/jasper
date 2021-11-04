using Jasper.Attributes;
using Jasper.AzureServiceBus;

[assembly: JasperModule(typeof(AzureServiceBusTransportExtension))]

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusTransportExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            // This will build and add the ASB transport
            // if it isn't already there'
            options.Endpoints.AsbTransport();
        }
    }
}
