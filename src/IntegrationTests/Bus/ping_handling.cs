using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Http.Transport;
using Jasper.Testing.Bus.Lightweight.Protocol;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace IntegrationTests.Bus
{
    public class ping_handling
    {
        [Fact]
        public async Task ping_sad_path_with_tcp()
        {
            var sender = new BatchedSender("tcp://localhost:2222".ToUri(), new SocketSenderProtocol(),
                CancellationToken.None, CompositeTransportLogger.Empty());

            await Jasper.Testing.Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () =>
            {
                await sender.Ping();
            });
        }

        [Fact]
        public async Task ping_happy_path_with_tcp()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Transports.LightweightListenerAt(2222);
            }))
            {
                var sender = new BatchedSender("tcp://localhost:2222".ToUri(), new SocketSenderProtocol(),
                    CancellationToken.None, CompositeTransportLogger.Empty());

                sender.Start(new StubSenderCallback());

                await sender.Ping();
            }
        }

        [Fact]
        public async Task ping_sad_path_with_http_protocol()
        {
            var sender = new BatchedSender("http://localhost:5005/messages".ToUri(), new HttpSenderProtocol(new BusSettings()), CancellationToken.None, CompositeTransportLogger.Empty());



            await Jasper.Testing.Exception<HttpRequestException>.ShouldBeThrownByAsync(async () =>
            {
                await sender.Ping();
            });
        }

        [Fact]
        public async Task ping_happy_path_with_http_protocol()
        {
            var sender = new BatchedSender("http://localhost:5005/messages".ToUri(), new HttpSenderProtocol(new BusSettings()), CancellationToken.None, CompositeTransportLogger.Empty());


            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Transports.Http.Enable(true);
                _.Http.UseUrls("http://localhost:5005").UseKestrel();
            }))
            {
                await sender.Ping();
            }
        }
    }
}
