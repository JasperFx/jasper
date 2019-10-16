using IntegrationTests;
using Jasper.Configuration;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class ItemReceiver : JasperRegistry
    {
        public ItemReceiver()
        {
            Handlers.Worker("items").IsDurable()
                .HandlesMessage<ItemCreated>();

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);
        }
    }
}
