using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Handlers.DisableConventionalDiscovery();

            Publish.Message<TraceMessage>().To(ReceiverApp.Listener);

            Settings.ConfigureMarten(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Include<MartenBackedPersistence>();

            Settings.Alter<JasperOptions>(_ =>
            {
                _.ScheduledJobs.PollingTime = 1.Seconds();
                _.ScheduledJobs.FirstExecution = 0.Seconds();
            });
        }
    }
}
