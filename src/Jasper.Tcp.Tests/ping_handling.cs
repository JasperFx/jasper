using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Tcp.Tests.Protocol;
using Jasper.Transports.Sending;
using Jasper.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Tcp.Tests
{
    public class ping_handling
    {
        [Fact]
        public async Task ping_happy_path_with_tcp()
        {
            using (var runtime = JasperHost.For(opts => { opts.ListenAtPort(2222); }))
            {
                var sender = new BatchedSender("tcp://localhost:2222".ToUri(), new SocketSenderProtocol(),
                    CancellationToken.None, NullLogger.Instance);

                sender.RegisterCallback(new StubSenderCallback());

                await sender.Ping(CancellationToken.None);
            }
        }

        [Fact]
        public async Task ping_sad_path_with_tcp()
        {
            var sender = new BatchedSender("tcp://localhost:3322".ToUri(), new SocketSenderProtocol(),
                CancellationToken.None, NullLogger.Instance);

            await Should.ThrowAsync<InvalidOperationException>(async () => { await sender.Ping(CancellationToken.None); });
        }
    }
}
