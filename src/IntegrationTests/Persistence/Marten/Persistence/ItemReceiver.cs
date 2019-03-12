using Jasper;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Persistence.Marten.Persistence
{
    public class ItemReceiver : JasperRegistry
    {
        public ItemReceiver()
        {
            Handlers.Worker("items").IsDurable()
                .HandlesMessage<ItemCreated>();

            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);

            HttpRoutes.DisableConventionalDiscovery();
        }
    }
}
