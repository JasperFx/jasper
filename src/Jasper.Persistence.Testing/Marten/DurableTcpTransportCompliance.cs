using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Marten;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{


    public class PortFinder
    {
        private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

        public static int GetAvailablePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(DefaultLoopbackEndpoint);
            var port = ((IPEndPoint)socket.LocalEndPoint).Port;
            return port;
        }
    }

    [Collection("marten")]
    public class DurableTcpTransportFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public DurableTcpTransportFixture() : base($"tcp://localhost:{ PortFinder.GetAvailablePort()}/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            OutboundAddress = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();

            await SenderIs(opts =>
            {
                var receivingUri = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();
                opts.ListenForMessagesFrom(receivingUri);

                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();
            });

            await ReceiverIs(opts =>
            {
                opts.ListenForMessagesFrom(OutboundAddress);

                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.DatabaseSchemaName = "receiver";
                }).IntegrateWithJasper();
            });
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }

    [Collection("marten")]
    public class DurableTcpTransportCompliance : SendingCompliance<DurableTcpTransportFixture>
    {

    }
}
