using IntegrationTests;
using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Tracking;
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

            Endpoints.ListenAtPort(2345).DurablyPersistedLocally();

            Extensions.UseMessageTrackingTestingSupport();

        }
    }
}
