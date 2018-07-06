using System;
using Jasper;
using Jasper.Marten.Tests;
using Jasper.SqlServer;
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
