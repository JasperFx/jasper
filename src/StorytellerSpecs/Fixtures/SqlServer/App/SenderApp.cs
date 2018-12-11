using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.SqlServer;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Handlers.DisableConventionalDiscovery();

            Publish.Message<TraceMessage>().To(ReceiverApp.Listener);

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "sender");

            Settings.Alter<JasperOptions>(_ =>
            {
                _.ScheduledJobs.PollingTime = 1.Seconds();
                _.ScheduledJobs.FirstExecution = 0.Seconds();
            });
        }
    }
}
