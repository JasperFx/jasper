using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Testing.Messaging;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Marten.Tests.Persistence
{
    public class ItemReceiver : JasperRegistry
    {
        public ItemReceiver()
        {
            Processing.Worker("items").IsDurable()
                .HandlesMessage<ItemCreated>();

            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);


        }
    }
}
