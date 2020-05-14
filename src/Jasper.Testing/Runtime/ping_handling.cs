using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Testing.Transports.Tcp.Protocol;
using Jasper.Transports.Sending;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class ping_handling
    {
        [Fact]
        public async Task ping_happy_path_with_tcp()
        {
            using (var runtime = JasperHost.For(_ => { _.Endpoints.ListenAtPort(2222); }))
            {
                var sender = new BatchedSender("tcp://localhost:2222".ToUri(), new SocketSenderProtocol(),
                    CancellationToken.None, TransportLogger.Empty());

                sender.RegisterCallback(new StubSenderCallback());

                await sender.Ping(CancellationToken.None);
            }
        }

        [Fact]
        public async Task ping_sad_path_with_tcp()
        {
            var sender = new BatchedSender("tcp://localhost:3322".ToUri(), new SocketSenderProtocol(),
                CancellationToken.None, TransportLogger.Empty());

            await Should.ThrowAsync<InvalidOperationException>(async () => { await sender.Ping(CancellationToken.None); });
        }
    }
}
