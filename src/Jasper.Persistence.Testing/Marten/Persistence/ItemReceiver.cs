using IntegrationTests;
using Jasper.Configuration;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class ItemReceiver : JasperOptions
    {
        public ItemReceiver()
        {
            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "receiver";
            });

            Services.AddSingleton<MessageTracker>();

            Endpoints.ListenAtPort(2345).Durably();

        }
    }
}
