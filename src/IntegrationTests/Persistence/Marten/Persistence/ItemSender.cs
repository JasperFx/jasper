using IntegrationTests.Persistence.Marten.Persistence.Resiliency;
using Jasper;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.Marten;
using Marten;
using Servers;

namespace IntegrationTests.Persistence.Marten.Persistence
{
    public class ItemSender : JasperRegistry
    {
        public ItemSender()
        {
            Include<MartenBackedPersistence>();
            Publish.Message<ItemCreated>().To("tcp://localhost:2345/durable");
            Publish.Message<Question>().To("tcp://localhost:2345/durable");

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(MartenContainer.ConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Transports.LightweightListenerAt(2567);
        }
    }
}
