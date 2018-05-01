using System.Threading.Tasks;
using Jasper.Http.Transport;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Transport
{
    public class too_busy_indication
    {
        [Fact]
        public async Task return_503_when_too_busy_lightweight()
        {
            var root = Substitute.For<IMessagingRoot>();
            root.ListeningStatus.Returns(ListeningStatus.TooBusy);

            var status = await TransportEndpoint.put__messages(null, null, null, root);

            status.ShouldBe(503);
        }

        [Fact]
        public async Task return_503_when_too_busy_durable()
        {
            var root = Substitute.For<IMessagingRoot>();
            root.ListeningStatus.Returns(ListeningStatus.TooBusy);

            var status = await TransportEndpoint.put__messages_durable(null, null, null, root);

            status.ShouldBe(503);
        }
    }
}
