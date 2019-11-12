using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class SenderApp : JasperOptions
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

            Advanced.ScheduledJobPollingTime = 1.Seconds();
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
        }
    }
}
