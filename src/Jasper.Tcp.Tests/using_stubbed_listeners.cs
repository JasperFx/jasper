using System.Threading.Tasks;
using Jasper.Tracking;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Tcp.Tests
{
    public class using_stubbed_listeners
    {
        #region sample_using_stubbed_listeners
        [Fact]
        public async Task track_outgoing_to_tcp_when_stubbed()
        {
            using var host = JasperHost.For(options =>
            {
                options.PublishAllMessages().ToPort(7777);
                options.StubAllExternallyOutgoingEndpoints();
                options.Extensions.UseMessageTrackingTestingSupport();
            });

            var message = new Message1();

            // The session can be interrogated to see
            // what activity happened while the tracking was
            // ongoing
            var session = await host.SendMessageAndWaitAsync(message);

            session.FindSingleTrackedMessageOfType<Message1>(EventType.Sent)
                .ShouldBeSameAs(message);
        }
        #endregion
    }
}
