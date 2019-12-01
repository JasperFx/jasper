using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp.Protocol
{
    public class error_in_receiver : ProtocolContext
    {
        public error_in_receiver()
        {
            theReceiver.ThrowErrorOnReceived = true;
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

        [Fact]
        public async Task logs_the_exception_on_the_receiving_side()
        {
            await afterSending();
            theReceiver.FailureException.ShouldBeOfType<DivideByZeroException>();
        }
    }
}
