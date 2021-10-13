using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    public class Sender : JasperOptions
    {
        public Sender(int portNumber)
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:{portNumber}/incoming".ToUri());
        }

        public Sender()
            : this(2389)
        {

        }
    }

    public class Receiver : JasperOptions
    {
        public Receiver(int portNumber)
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:{portNumber}/incoming".ToUri());
        }

        public Receiver() : this(2388)
        {

        }
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

    public class LightweightTcpFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public LightweightTcpFixture() : base($"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            await SenderIs(new Sender(PortFinder.GetAvailablePort()));

            await ReceiverIs(new Receiver(OutboundAddress.Port));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }


    [Collection("compliance")]
    public class LightweightTcpTransportCompliance : SendingCompliance<LightweightTcpFixture>
    {
        public LightweightTcpTransportCompliance(LightweightTcpFixture fixture) : base(fixture)
        {
        }
    }
}
