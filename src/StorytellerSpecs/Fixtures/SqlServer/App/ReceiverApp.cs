using System;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Util;
using Servers;
using Servers.Docker;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class ReceiverApp : JasperRegistry
    {
        public readonly static Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Settings.PersistMessagesWithSqlServer(SqlServerContainer.ConnectionString, "receiver");

            Transports.ListenForMessagesFrom(Listener);
        }
    }
}
