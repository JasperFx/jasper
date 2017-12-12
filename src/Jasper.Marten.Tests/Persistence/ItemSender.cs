using Jasper.Marten.Tests.Setup;
using Marten;

namespace Jasper.Marten.Tests.Persistence
{
    public class ItemSender : JasperRegistry
    {
        public ItemSender()
        {
            Include<MartenBackedPersistence>();
            Publish.Message<ItemCreated>().To("tcp://localhost:2345/durable");

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "sender";
            });


        }
    }
}
