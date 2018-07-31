using Jasper;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Servers;
using Servers.Docker;

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
                _.Connection(MartenContainer.ConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);
        }
    }
}
