using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Messaging.Transports.Configuration;
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

            Settings.Alter<MessagingSettings>(_ =>
            {
                _.ScheduledJobPollingTime = 1.Seconds();
                _.FirstScheduledJobExecution = 0.Seconds();
            });
        }
    }
}
