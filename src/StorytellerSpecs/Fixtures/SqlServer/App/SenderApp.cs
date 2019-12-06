using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.SqlServer;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class SenderApp : JasperOptions
    {
        public SenderApp()
        {
            Handlers.DisableConventionalDiscovery();

            Endpoints.Publish(x => x.Message<TraceMessage>().To(ReceiverApp.Listener));

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "sender");

            Advanced.ScheduledJobPollingTime = 1.Seconds();
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
        }
    }
}
