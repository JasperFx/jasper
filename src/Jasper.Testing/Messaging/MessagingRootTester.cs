using Baseline;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class MessagingRootTester
    {
        [Fact]
        public void create_bus_for_envelope()
        {
            var root = new MockMessagingRoot();
            var original = ObjectMother.Envelope();

            var bus = root.ContextFor(original);

            bus.Envelope.ShouldBe(original);
            bus.EnlistedInTransaction.ShouldBeTrue();

            bus.As<MessageContext>().Transaction.ShouldBeOfType<InMemoryEnvelopeTransaction>();
        }
    }
}
