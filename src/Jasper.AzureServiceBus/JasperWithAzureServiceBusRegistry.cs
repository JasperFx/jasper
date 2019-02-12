using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
            Settings.Alter<AzureServiceBusSettings>((context, settings) =>
            {
                Configure(context.HostingEnvironment, context.Configuration, settings);
            });
        }

        /// <summary>
        /// Override to configure the Azure Service Bus topology
        /// </summary>
        /// <param name="contextHostingEnvironment"></param>
        /// <param name="configuration"></param>
        /// <param name="settings"></param>
        protected abstract void Configure(IHostingEnvironment contextHostingEnvironment, IConfiguration configuration,
            AzureServiceBusSettings settings);
    }
}
