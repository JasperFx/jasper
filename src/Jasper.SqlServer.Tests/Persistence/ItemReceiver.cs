using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servers;

namespace Jasper.SqlServer.Tests.Persistence
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
