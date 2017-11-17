using Jasper.Settings;
using Marten;

namespace Jasper.Marten
{
    public static class JasperRegistryExtensions
    {
        public static void MartenConnectionStringIs(this JasperSettings settings, string connectionString)
        {
            settings.Alter<StoreOptions>(x => x.Connection(connectionString));
        }

        public static void MartenConnectionStringIs(this JasperRegistry registry, string connectionString)
        {
            registry.Settings.MartenConnectionStringIs(connectionString);
        }
    }
}
