using System;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;
using Jasper.Util;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class ReceiverApp : JasperRegistry
    {
        public static readonly Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Settings.ConfigureMarten(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.DatabaseSchemaName = "receiver";
            });

            Include<MartenBackedPersistence>();

            Transports.ListenForMessagesFrom(Listener);
        }
    }
}
