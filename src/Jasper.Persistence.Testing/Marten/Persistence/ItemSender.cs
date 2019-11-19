using IntegrationTests;
using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Testing.Marten.Persistence.Resiliency;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class ItemSender : JasperOptions
    {
        public ItemSender()
        {
            Include<MartenBackedPersistence>();
            Publish.Message<ItemCreated>().DurablyToPort(2345);

            Publish.Message<Question>().DurablyToPort(2345);

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Transports.LightweightListenerAt(2567);

        }
    }
}
