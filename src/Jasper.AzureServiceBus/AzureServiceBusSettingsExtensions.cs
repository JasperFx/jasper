using Jasper.Settings;

namespace Jasper.AzureServiceBus
{
    public static class AzureServiceBusSettingsExtensions
    {
        public static void AddAzureServiceBusConnection(this JasperSettings settings, string name,
            string connectionString)
        {
            settings.Alter<AzureServiceBusSettings>(s => s.Connections.Add(name, connectionString));
        }
    }
}
