using Jasper;
using Jasper.AzureServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


namespace JasperService
{
    internal class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            // Any static configuration that does not depend
            // on the environment or configuration. Can be omitted.
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // You must supply a connection string to Azure Service Bus
            Endpoints.ConfigureAzureServiceBus(config.GetValue<string>("AzureServiceBusConnectionString"));

            // Listen for incoming messages from a named queue
            Endpoints.ListenToAzureServiceBusQueue("incoming");
            Endpoints.PublishAllMessages().ToAzureServiceBusQueue("outgoing");

            
            

        }
    }

}