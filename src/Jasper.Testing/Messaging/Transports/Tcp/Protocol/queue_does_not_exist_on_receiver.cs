using System.Threading.Tasks;
using Jasper.Messaging.Transports.Tcp;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp.Protocol
{
    public class queue_does_not_exist_on_receiver : ProtocolContext
    {
        public queue_does_not_exist_on_receiver()
        {
            theReceiver.StatusToReturn = ReceivedStatus.QueueDoesNotExist;
        }

        [Fact]
        public async Task did_not_succeed()
        {
            await afterSending();
            theSender.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public async Task should_tell_the_sender_callback()
        {
            await afterSending();
            theSender.QueueDoesNotExist.ShouldBeTrue();
        }
    }
}
