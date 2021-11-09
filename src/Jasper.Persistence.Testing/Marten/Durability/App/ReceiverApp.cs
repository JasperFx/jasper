using System;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Tcp;
using Jasper.Util;
using TestingSupport;

namespace Jasper.Persistence.Testing.Marten.Durability.App
{
    public class ReceiverApp : JasperOptions
    {
        public static readonly Uri Listener = $"tcp://localhost:{PortFinder.GetAvailablePort()}/durable".ToUri();

        public ReceiverApp()
        {
            Extensions.Include<TcpTransportExtension>();

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
