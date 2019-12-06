using System;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;
using Jasper.Util;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class ReceiverApp : JasperOptions
    {
        public static readonly Uri Listener = "tcp://localhost:2555/durable".ToUri();

        public ReceiverApp()
        {
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Extensions.UseMarten(o =>
            {
                o.Connection(Servers.PostgresConnectionString);
                o.DatabaseSchemaName = "receiver";
            });

            Endpoints.ListenForMessagesFrom(Listener);
        }
    }
}
