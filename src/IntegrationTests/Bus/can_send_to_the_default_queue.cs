using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace IntegrationTests.Bus
{
    public class can_send_to_the_default_queue : SendingContext
    {
        [Fact]
        public async Task send_to_the_default_queue()
        {
            StartTheReceiver(_ =>
            {
                _.Transports.ListenForMessagesFrom("tcp://localhost:2255");
            });

            StartTheSender(_ =>
            {
                _.Publish.AllMessagesTo("tcp://localhost:2255");
            });

            var waiter = theTracker.WaitFor<Message1>();

            var message1 = new Message1();
            await theSender.Bus.Send(message1);

            var envelope = await waiter;

            envelope.Message.As<Message1>().Id.ShouldBe(message1.Id);
        }

        [Fact]
        public async Task can_still_receive_if_the_queue_does_not_exist()
        {
            StartTheReceiver(_ =>
            {
                _.Transports.ListenForMessagesFrom("tcp://localhost:2266");
            });

            StartTheSender(_ =>
            {
                _.Publish.AllMessagesTo("tcp://localhost:2266/unknown");
            });

            var waiter = theTracker.WaitFor<Message1>();

            var message1 = new Message1();
            await theSender.Bus.Send(message1);

            var envelope = await waiter;

            envelope.Message.As<Message1>().Id.ShouldBe(message1.Id);
        }
    }
}
