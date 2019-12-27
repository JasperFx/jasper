using System.Threading.Tasks;
using Jasper.Tracking;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class can_send_to_the_default_queue : SendingContext
    {
        [Fact]
        public async Task can_still_receive_if_the_queue_does_not_exist()
        {
            StartTheReceiver(_ => { _.Endpoints.ListenAtPort(2270); });

            StartTheSender(_ => { _.Endpoints.PublishAllMessages().To("tcp://localhost:2270/unknown"); });

            var message1 = new Message1();

            var session = await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .SendMessageAndWait(message1);

            session.FindSingleTrackedMessageOfType<Message1>(EventType.Received)
                .Id.ShouldBe(message1.Id);

        }

        [Fact]
        public async Task send_to_the_default_queue()
        {
            StartTheReceiver(_ => { _.Endpoints.ListenAtPort(2258); });

            StartTheSender(_ => { _.Endpoints.PublishAllMessages().To("tcp://localhost:2258"); });

            var message1 = new Message1();

            var session = await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .SendMessageAndWait(message1);


            session.FindSingleTrackedMessageOfType<Message1>(EventType.Received)
                .Id.ShouldBe(message1.Id);
        }
    }
}
