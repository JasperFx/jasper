using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace IntegrationTests.Lightweight.Protocol
{
    public class super_duper_happy_path : ProtocolContext
    {
        [Fact]
        public async Task messages_are_received()
        {
            await afterSending();

            allTheMessagesWereReceived();
        }

        [Fact]
        public async Task acknowledgements_were_received()
        {
            await afterSending();

            theReceiver.WasAcknowledged.ShouldBe(true);
        }

        [Fact]
        public async Task should_call_through_succeeded()
        {
            await afterSending();

            theSender.Succeeded.ShouldBeTrue();
        }
    }
}
