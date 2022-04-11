using System;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;
using Jasper.Tcp;
using Jasper.Util;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class ReceiverApp : JasperOptions
    {
        public ReceiverApp(Uri listener)
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Extensions.UseMarten(o =>
            {
                o.Connection(Servers.PostgresConnectionString);
                o.DatabaseSchemaName = "receiver";
            });

            ListenForMessagesFrom(listener).DurablyPersistedLocally();
        }
    }
}
