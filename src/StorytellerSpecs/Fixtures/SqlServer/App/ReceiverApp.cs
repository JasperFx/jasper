using System;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Util;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class ReceiverApp : JasperOptions
    {
        public static readonly Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            Transports.ListenForMessagesFrom(Listener);
        }
    }
}
