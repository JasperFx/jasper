using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Subscriptions
{
    public class SubscriptionTester
    {
        [Fact]
        public void new_subscription_uses_the_type_alias()
        {
            new Subscription(typeof(Message1))
                .MessageType.ShouldBe(typeof(Message1).FullName);

            new Subscription(typeof(AliasedMessage))
                .MessageType.ShouldBe("Alias.1");
        }

        [Fact]
        public void new_subscriptions_accept_json_by_default()
        {
            new Subscription(typeof(Message1)).Accepts
                .ShouldBe("application/json");
        }
    }

    [TypeAlias("Alias.1")]
    public class AliasedMessage
    {

    }
}
