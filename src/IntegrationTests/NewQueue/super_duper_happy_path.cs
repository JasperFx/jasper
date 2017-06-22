using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests.NewQueue
{
    public class super_duper_happy_path : ProtocolContext
    {
        [Fact]
        public async Task messages_are_received()
        {
            await afterSending();

            allTheMessagesWereReceived();
        }
    }
}