using System;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Tcp;
using Jasper.Util;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class ReceiverApp : JasperOptions
    {
        public ReceiverApp(Uri listener)
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            Endpoints.ListenForMessagesFrom(listener).DurablyPersistedLocally();
        }
    }
}
