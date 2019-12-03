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
            Publish.Message<ItemCreated>().ToPort(2345).Durably();

            Publish.Message<Question>().ToPort(2345).Durably();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Endpoints.ListenAtPort(2567);

        }
    }
}
