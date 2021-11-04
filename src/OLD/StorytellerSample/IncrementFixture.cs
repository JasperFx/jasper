using System.Threading.Tasks;
using Jasper.Messaging.Tracking;
using Jasper.TestSupport.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    // SAMPLE: IncrementFixture
    public class IncrementFixture : MessagingFixture
    {
        [FormatAs("Send increment message from the other application")]
        public Task SendIncrementMessage()
        {
            // Just to show the functionality, you can get at the JasperRuntime
            // -- and therefore everything about the other app -- by
            // using the NodeFor() method as shown below:
            var node = NodeFor("other");


            // This sends a message from the external node named "Other"
            return node.Host.SendMessageAndWait(new Increment());
        }

        [FormatAs("The current count should be {count}")]
        public int TheIncrementCountShouldBe()
        {
            var counter = Context.Service<IncrementCounter>();
            return counter.Count;
        }
    }

    // ENDSAMPLE
}
