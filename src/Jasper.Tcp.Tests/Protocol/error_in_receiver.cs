using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Tcp.Tests.Protocol
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


    }
}
