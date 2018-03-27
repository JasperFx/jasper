using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Transports.Tcp;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Lightweight.Protocol
{
    public class receiver_says_that_the_process_failed_cleanly : ProtocolContext
    {
        public receiver_says_that_the_process_failed_cleanly()
        {
            theReceiver.StatusToReturn = ReceivedStatus.ProcessFailure;
        }

        [Fact]
        public async Task did_not_succeed()
        {
            await afterSending();
            theSender.Succeeded.ShouldBeFalse();
        }



        [Fact]
        public async Task logs_processing_failure_in_sender()
        {
            await afterSending();
            theSender.ProcessingFailed.ShouldBeTrue();
        }
    }
}
