using System;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.AzureServiceBus
{
    public static class AzureServiceBusSettingsExtensions
    {
        /// <summary>
        /// Add a new, named Azure Service Bus connection to the application
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        public static void AddAzureServiceBusConnection(this SettingsGraph settings, string name,
            string connectionString)
        {
            settings.Alter<AzureServiceBusOptions>(s => s.Connections.Add(name, connectionString));
        }

        /// <summary>
        /// Configure the options for the Azure Service Bus transport
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="alteration"></param>
        public static void ConfigureAzureServiceBus(this SettingsGraph settings, Action<AzureServiceBusOptions> alteration)
        {
            settings.Alter(alteration);
        }

        /// <summary>
        /// Configure the options for the Azure Service Bus transport using the system's IConfiguration and IHostingEnvironment
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="alteration"></param>
        public static void ConfigureAzureServiceBus(this SettingsGraph settings, Action<AzureServiceBusOptions, IHostEnvironment, IConfiguration> alteration)
        {
            settings.Alter<AzureServiceBusOptions>((context, x) => alteration(x, context.HostingEnvironment, context.Configuration));
        }
    }
}
