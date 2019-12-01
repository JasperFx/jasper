using System;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp
{
    public class TcpEndpointTests
    {
        [Fact]
        public void default_host()
        {
            new TcpEndpoint()
                .HostName.ShouldBe("localhost");

            new TcpEndpoint(3333)
                .HostName.ShouldBe("localhost");
        }

        [Theory]
        [InlineData("tcp://localhost:4444", "localhost", 4444, false)]
        [InlineData("tcp://localhost:4445", "localhost", 4445, false)]
        [InlineData("tcp://server1:4445", "server1", 4445, false)]
        [InlineData("tcp://server1:4445/durable", "server1", 4445, true)]
        public void parsing_uri(string uri, string host, int port, bool isDurable)
        {
            var endpoint = new TcpEndpoint();
            endpoint.Parse(uri.ToUri());

            endpoint.HostName.ShouldBe(host);
            endpoint.Port.ShouldBe(port);
            endpoint.IsDurable.ShouldBe(isDurable);
        }


    }
}
