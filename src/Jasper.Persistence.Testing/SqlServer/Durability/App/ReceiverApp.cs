using System;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.Testing.Marten;
using Jasper.Tcp;
using Jasper.Util;
using TestingSupport;

namespace Jasper.Persistence.Testing.SqlServer.Durability.App
{
    public class ReceiverApp : JasperOptions
    {
        public static readonly Uri Listener = $"tcp://localhost:{PortFinder.GetAvailablePort()}/durable".ToUri();

        public ReceiverApp()
        {
            Extensions.Include<TcpTransportExtension>();
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<TraceHandler>();

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

            Endpoints.ListenForMessagesFrom(Listener);
        }
    }
}
