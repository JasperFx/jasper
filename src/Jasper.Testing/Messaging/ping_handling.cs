using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Testing.Messaging.Lightweight.Protocol;
using Jasper.Util;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class ping_handling
    {
        [Fact]
        public async Task ping_happy_path_with_tcp()
        {
            var runtime = await JasperRuntime.ForAsync(_ => { _.Transports.LightweightListenerAt(2222); });

            try
            {
                var sender = new BatchedSender("tcp://localhost:2222".ToUri(), new SocketSenderProtocol(),
                    CancellationToken.None, TransportLogger.Empty());

                sender.Start(new StubSenderCallback());

                await sender.Ping();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task ping_sad_path_with_tcp()
        {
            var sender = new BatchedSender("tcp://localhost:3322".ToUri(), new SocketSenderProtocol(),
                CancellationToken.None, TransportLogger.Empty());

            await Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () => { await sender.Ping(); });
        }
    }
}
