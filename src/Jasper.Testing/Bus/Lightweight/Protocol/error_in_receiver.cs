using System;
using Baseline.Dates;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Lightweight.Protocol
{
    public class error_in_receiver : ProtocolContext
    {
        public error_in_receiver()
        {
            theReceiver.ThrowErrorOnReceived = true;

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

        [Fact]
        public void logs_the_exception_on_the_receiving_side()
        {
            theReceiver.FailureException.ShouldBeOfType<DivideByZeroException>();
        }
    }
}