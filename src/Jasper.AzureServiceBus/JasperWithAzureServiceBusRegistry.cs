using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.AzureServiceBus
{
    /// <summary>
    /// Can be used as a recipe for integrating Azure Service Bus with Jasper through
    /// application configuration
    /// </summary>
    public abstract class JasperWithAzureServiceBusRegistry : JasperRegistry
    {
        public JasperWithAzureServiceBusRegistry()
        {
            Settings.Alter<AzureServiceBusOptions>((context, settings) =>
            {
                Configure(context.HostingEnvironment, context.Configuration, settings);
            });
        }

        /// <summary>
        /// Override to configure the Azure Service Bus topology
        /// </summary>
        /// <param name="contextHostingEnvironment"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        protected abstract void Configure(IHostEnvironment contextHostingEnvironment, IConfiguration configuration,
            AzureServiceBusOptions options);
    }
}
