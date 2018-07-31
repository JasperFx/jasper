using Jasper;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Servers;
using Servers.Docker;

namespace IntegrationTests.Persistence.SqlServer.Persistence
{
    public class ItemReceiver : JasperRegistry
    {
        public ItemReceiver()
        {
            Handlers.Worker("items").IsDurable()
                .HandlesMessage<ItemCreated>();

            Settings.PersistMessagesWithSqlServer(SqlServerContainer.ConnectionString, "receiver");

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);


        }
    }
}
