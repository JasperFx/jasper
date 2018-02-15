using Baseline.Dates;
using Jasper.Messaging.Transports.Tcp;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Lightweight.Protocol
{
    [Collection("integration")]
    public class queue_does_not_exist_on_receiver : ProtocolContext
    {
        public queue_does_not_exist_on_receiver()
        {
            theReceiver.StatusToReturn = ReceivedStatus.QueueDoesNotExist;

            afterSending().Wait(2.Seconds());
        }

        [Fact]
        public void did_not_succeed()
        {
            theSender.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public void should_tell_the_sender_callback()
        {
            theSender.QueueDoesNotExist.ShouldBeTrue();
        }
    }
}
