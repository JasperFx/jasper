using System.Threading.Tasks;
using Jasper.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    public class IncrementFixture : MessagingFixture
    {
        [FormatAs("Send increment message from the other application")]
        public Task SendIncrementMessage()
        {
            return SendMessageAndWaitForCompletion("Other", new Increment());
        }

        [FormatAs("The current count should be {count}")]
        public int TheIncrementCountShouldBe()
        {
            var counter = Context.Service<IncrementCounter>();
            return counter.Count;
        }
    }
}
