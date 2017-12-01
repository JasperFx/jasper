using Baseline.Dates;
using Jasper;
using Jasper.Bus.Transports.Configuration;
using Jasper.Marten;
using Jasper.Marten.Tests.Setup;

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

            Settings.Alter<BusSettings>(_ =>
            {
                _.ScheduledJobPollingTime = 1.Seconds();
                _.FirstScheduledJobExecution = 0.Seconds();
            });
        }
    }
}
