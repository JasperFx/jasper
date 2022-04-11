using System;
using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;
using Jasper.Tcp;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class SenderApp : JasperOptions
    {
        public SenderApp(Uri listener)
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();

            Publish(x => x.Message<TraceMessage>().To(listener).Durably());

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
