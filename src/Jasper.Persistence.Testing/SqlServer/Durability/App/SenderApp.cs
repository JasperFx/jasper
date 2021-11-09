using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Tcp;
using TestingSupport;

namespace Jasper.Persistence.Testing.SqlServer.Durability.App
{
    public class SenderApp : JasperOptions
    {
        public SenderApp()
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();

            Endpoints.Publish(x => x.Message<TraceMessage>().To(ReceiverApp.Listener));

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "sender");

            Advanced.ScheduledJobPollingTime = 1.Seconds();
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
        }
    }
}
