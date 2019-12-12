using System.Threading.Tasks;
using Jasper.Messaging.Tracking;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Tracking
{
    public class using_stubbed_listeners
    {
        [Fact]
        public async Task track_outgoing_to_tcp_when_stubbed()
        {
            using (var host = JasperHost.For(options =>
            {
                options.Endpoints.PublishAllMessages().ToPort(7777);
                options.Endpoints.StubAllExternallyOutgoingEndpoints();
                options.Extensions.UseMessageTrackingTestingSupport();
            }))
            {
                var message = new Message1();

                var session = await host.SendMessageAndWait(message);

                session.FindSingleTrackedMessageOfType<Message1>(EventType.Sent)
                    .ShouldBeSameAs(message);
            }
        }
    }
}
