using System;
using Jasper;
using Jasper.Marten;
using Jasper.Marten.Tests;
using Jasper.Util;
using Servers;

namespace DurabilitySpecs.Fixtures.Marten.App
{
    public class ReceiverApp : JasperRegistry
    {
        public readonly static Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Settings.ConfigureMarten(_ =>
            {
                _.Connection(MartenContainer.ConnectionString);
                _.DatabaseSchemaName = "receiver";

            });

            Include<MartenBackedPersistence>();

            Transports.ListenForMessagesFrom(Listener);
        }
    }
}
