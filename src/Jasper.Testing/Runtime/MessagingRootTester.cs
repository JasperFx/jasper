using Baseline;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime
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

            bus.As<ExecutionContext>().Transaction.ShouldBeOfType<InMemoryEnvelopeTransaction>();
        }
    }
}
