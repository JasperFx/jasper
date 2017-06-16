using Jasper.Bus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionMatchesTester
    {
        [Fact]
        public void fuzzy_matches()
        {
            var subscription = Subscription.For<Message1>();

            ShouldBeBooleanExtensions.ShouldBeTrue(subscription.Matches(typeof(Message1)));
            ShouldBeBooleanExtensions.ShouldBeFalse(subscription.Matches(typeof(Message2)));
        }
    }
}
