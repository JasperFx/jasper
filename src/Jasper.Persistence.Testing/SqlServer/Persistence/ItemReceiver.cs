using IntegrationTests;
using Jasper.Configuration;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class ItemReceiver : JasperOptions
    {
        public ItemReceiver()
        {

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            Services.AddSingleton<MessageTracker>();

            Transports.DurableListenerAt(2345);
        }
    }
}
