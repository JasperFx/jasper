using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Receiving
{
    public class SocketListeningAgentTests
    {
        [Fact]
        public async Task receive_when_it_is_latched()
        {
            var stream = new MemoryStream();

            var agent = new SocketListener(IPAddress.Any, 5500, CancellationToken.None);
            agent.Status = ListeningStatus.TooBusy;

            var callback = Substitute.For<IReceiverCallback>();

            await agent.HandleStream(callback, stream);

            stream.Position = 0;
            var bytes = stream.ReadAllBytes();

            bytes.ShouldBe(WireProtocol.ProcessingFailureBuffer);

            callback.DidNotReceive().Received();
        }

        [Fact]
        public void status_is_accepting_by_default()
        {
            new SocketListener(IPAddress.Any, 5500, CancellationToken.None)
                .Status.ShouldBe(ListeningStatus.Accepting);
        }
    }
}
