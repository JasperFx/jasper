using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Tcp.Tests
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

    public class LightweightTcpFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public LightweightTcpFixture() : base($"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            await SenderIs(opts =>
            {
                opts.ListenAtPort(PortFinder.GetAvailablePort());
            });

            await ReceiverIs(opts =>
            {
                opts.ListenAtPort(OutboundAddress.Port);
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }


    [Collection("compliance")]
    public class LightweightTcpTransportCompliance : SendingCompliance<LightweightTcpFixture>
    {

    }
}
