using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Transports;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Tcp.Tests
{
    public class SocketListeningAgentTests
    {
        [Fact]
        public async Task receive_when_it_is_latched()
        {
            var stream = new MemoryStream();

            var agent = new SocketListener(TransportLogger.Empty(), IPAddress.Any, 5500, CancellationToken.None);
            agent.Status = ListeningStatus.TooBusy;

            var callback = Substitute.For<IListeningWorkerQueue>();

            await agent.HandleStream(callback, stream);

            stream.Position = 0;
            var bytes = stream.ReadAllBytes();

            bytes.ShouldBe(WireProtocol.ProcessingFailureBuffer);

            callback.DidNotReceive().Received();
        }

        [Fact]
        public void status_is_accepting_by_default()
        {
            new SocketListener(TransportLogger.Empty(), IPAddress.Any, 5500, CancellationToken.None)
                .Status.ShouldBe(ListeningStatus.Accepting);
        }
    }
}
