using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class can_send_to_the_default_queue : SendingContext
    {
        [Fact]
        public async Task can_still_receive_if_the_queue_does_not_exist()
        {
            StartTheReceiver(_ => { _.Transports.ListenForMessagesFrom("tcp://localhost:2270"); });

            StartTheSender(_ => { _.Publish.AllMessagesTo("tcp://localhost:2270/unknown"); });

            var waiter = theTracker.WaitFor<Message1>();

            var message1 = new Message1();
            await theSender.Get<IMessagePublisher>().Send(message1);

            var envelope = await waiter;

            envelope.Message.As<Message1>().Id.ShouldBe(message1.Id);
        }

        [Fact]
        public async Task send_to_the_default_queue()
        {
            StartTheReceiver(_ => { _.Transports.ListenForMessagesFrom("tcp://localhost:2258"); });

            StartTheSender(_ => { _.Publish.AllMessagesTo("tcp://localhost:2258"); });

            var waiter = theTracker.WaitFor<Message1>();

            var message1 = new Message1();
            await theSender.Get<IMessagePublisher>().Send(message1);

            var envelope = await waiter;

            envelope.Message.As<Message1>().Id.ShouldBe(message1.Id);
        }
    }
}
