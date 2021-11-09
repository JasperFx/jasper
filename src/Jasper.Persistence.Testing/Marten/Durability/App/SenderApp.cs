using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Tcp;
using TestingSupport;

namespace Jasper.Persistence.Testing.Marten.Durability.App
{
    public class SenderApp : JasperOptions
    {
        public SenderApp()
        {
            Extensions.Include<TcpTransportExtension>();

            Handlers.DisableConventionalDiscovery();

            Endpoints.Publish(x => x.Message<TraceMessage>().To(ReceiverApp.Listener));

            Extensions.UseMarten(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Advanced.ScheduledJobPollingTime = 1.Seconds();
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
        }
    }
}
