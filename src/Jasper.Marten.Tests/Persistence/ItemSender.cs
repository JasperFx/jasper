using Jasper.Marten.Tests.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Transports.Configuration;
using Marten;

namespace Jasper.Marten.Tests.Persistence
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
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Transports.LightweightListenerAt(2567);
        }
    }
}
