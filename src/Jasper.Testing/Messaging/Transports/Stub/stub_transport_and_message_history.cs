using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Stub
{
    public class stub_transport_and_message_history
    {
        [Fact]
        public async Task can_use_stub_transport_to_catch_cascading_messages()
        {
            var runtime1 = JasperHost.For(x =>
            {
                x.Handlers.DisableConventionalDiscovery().IncludeType<QuestionAndAnswer>();
                x.Extensions.Include<MessageTrackingExtension>();
            });

            var tracker = runtime1.Get<MessageHistory>();

            var tracks = await tracker.WatchAsync(() => runtime1.Get<IMessagePublisher>().Send(new Question
            {
                One = 3,
                Two = 5
            }));

            // First envelope is sent & executed. Cascading message is sent and received
            tracks.Length.ShouldBe(2);
        }
    }
}
