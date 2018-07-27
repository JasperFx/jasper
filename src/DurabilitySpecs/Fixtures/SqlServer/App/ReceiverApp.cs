using System;
using IntegrationTests.Persistence.Marten;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Util;

namespace DurabilitySpecs.Fixtures.SqlServer.App
{
    public class ReceiverApp : JasperRegistry
    {
        public readonly static Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString, "receiver");

            Transports.ListenForMessagesFrom(Listener);
        }
    }
}
