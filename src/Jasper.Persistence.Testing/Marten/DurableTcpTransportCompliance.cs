using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Tcp;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class Sender : JasperOptions
    {
        public Sender()
        {
            Extensions.Include<TcpTransportExtension>();
            ReceivingUri = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();
            Endpoints.ListenForMessagesFrom(ReceivingUri);

            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "sender";
            });

        }

        public Uri ReceivingUri { get; set; }
    }

    public class Receiver : JasperOptions
    {
        public Receiver()
        {
            Extensions.Include<TcpTransportExtension>();
            ReceivingUri = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();
            Endpoints.ListenForMessagesFrom(ReceivingUri);



            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "receiver";
            });

        }

        public Uri ReceivingUri { get; set; }
    }

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
            var receiver = new Receiver();
            OutboundAddress = receiver.ReceivingUri;
            var sender = new Sender();
            await SenderIs(sender);

            await ReceiverIs(receiver);
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
