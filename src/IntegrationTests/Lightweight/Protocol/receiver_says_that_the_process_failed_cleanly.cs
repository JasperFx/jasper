using Baseline.Dates;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Lightweight;
using Shouldly;
using Xunit;

namespace IntegrationTests.Lightweight.Protocol
{
    public class receiver_says_that_the_process_failed_cleanly : ProtocolContext
    {
        public receiver_says_that_the_process_failed_cleanly()
        {
            theReceiver.StatusToReturn = ReceivedStatus.ProcessFailure;

            afterSending().Wait(2.Seconds());
        }

        [Fact]
        public void did_not_succeed()
        {
            theSender.Succeeded.ShouldBeFalse();
        }



        [Fact]
        public void logs_processing_failure_in_sender()
        {
            theSender.ProcessingFailed.ShouldBeTrue();
        }
    }
}