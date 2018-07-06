using Baseline.Dates;
using Jasper;
using Jasper.Marten.Tests;
using Jasper.Messaging.Transports.Configuration;
using Jasper.SqlServer;

namespace DurabilitySpecs.Fixtures.SqlServer.App
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Handlers.DisableConventionalDiscovery();

            Publish.Message<TraceMessage>().To(ReceiverApp.Listener);

            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString, "sender");

            Settings.Alter<MessagingSettings>(_ =>
            {
                _.ScheduledJobPollingTime = 1.Seconds();
                _.FirstScheduledJobExecution = 0.Seconds();
            });
        }
    }
}
