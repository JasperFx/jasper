using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Servers;

namespace Jasper.Marten.Tests.Persistence
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
