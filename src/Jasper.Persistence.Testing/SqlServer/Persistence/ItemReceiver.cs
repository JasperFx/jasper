using IntegrationTests;
using Jasper.Configuration;
using Jasper.Persistence.SqlServer;
using Jasper.Tcp;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class ItemReceiver : JasperOptions
    {
        public ItemReceiver()
        {

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            this.ListenAtPort(2345).DurablyPersistedLocally();
        }
    }
}
