using System;
using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Tcp;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class SenderApp : JasperOptions
    {
        public SenderApp(Uri listener)
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();

            Endpoints.Publish(x => x.Message<TraceMessage>().To(listener).Durably());

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "sender");

            Advanced.ScheduledJobPollingTime = 1.Seconds();
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
        }
    }
}
