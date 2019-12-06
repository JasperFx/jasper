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
