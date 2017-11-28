using System;
using Jasper;
using Jasper.Marten;
using Jasper.Marten.Tests.Setup;
using Jasper.Util;

namespace DurabilitySpecs.Fixtures.Marten.App
{
    public class ReceiverApp : JasperRegistry
    {
        public readonly static Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.IncludeType<TraceHandler>();

            Settings.ConfigureMarten(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            Include<MartenBackedPersistence>();
        }
    }
}