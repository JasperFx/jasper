using Baseline.Dates;
using Jasper;
using Jasper.Marten;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Transports.Configuration;
using Marten;

namespace DurabilitySpecs.Fixtures.Marten.App
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Publish.Message<TraceMessage>().To(ReceiverApp.Listener);

            Settings.ConfigureMarten(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "sender";
            });

            Include<MartenBackedPersistence>();

            Logging.UseConsoleLogging = true;

            Settings.Alter<MessagingSettings>(_ =>
            {
                _.ScheduledJobPollingTime = 1.Seconds();
                _.FirstScheduledJobExecution = 0.Seconds();
            });
        }
    }
}
