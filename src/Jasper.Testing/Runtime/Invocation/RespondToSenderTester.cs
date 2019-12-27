using Jasper.Runtime.Invocation;
using Jasper.Testing.Messaging;
using Jasper.Util;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime.Invocation
{
    public class RespondToSenderTester
    {
        [Fact]
        public void apply_the_destination()
        {
            var message1 = new Message1();

            var sender = new RespondToSender(message1);

            var original = ObjectMother.Envelope();
            original.ReplyUri = "tcp://server3:2222".ToUri();

            var created = sender.CreateEnvelope(original);

            created.Message.ShouldBe(message1);
            created.Destination.ShouldBe(original.ReplyUri);
        }
    }
}
