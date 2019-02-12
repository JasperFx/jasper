using Jasper.Settings;

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
        public static void AddAzureServiceBusConnection(this JasperSettings settings, string name,
            string connectionString)
        {
            settings.Alter<AzureServiceBusSettings>(s => s.Connections.Add(name, connectionString));
        }
    }
}
