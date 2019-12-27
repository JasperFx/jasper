using Jasper;
using Jasper.Configuration;
using Jasper.Runtime.Routing;
using Jasper.Testing.Runtime.Green;
using Jasper.Testing.Runtime.Red;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class SubscriptionTester
    {
        public class RandomClass{}

        [Fact]
        public void negative_assembly_test()
        {
            var rule = new Subscription(typeof(RandomClass).Assembly);
            rule.Matches(typeof(Message1)).ShouldBeFalse();
            rule.Matches(typeof(Message2)).ShouldBeFalse();
            rule.Matches(GetType()).ShouldBeTrue();
        }

        [Fact]
        public void negative_namespace_test()
        {
            var rule = new Subscription
            {
                Scope = RoutingScope.Namespace,
                Match = typeof(RedMessage1).Namespace
            };

            rule.Matches(typeof(GreenMessage1)).ShouldBeFalse();
            rule.Matches(typeof(GreenMessage2)).ShouldBeFalse();
            rule.Matches(typeof(GreenMessage3)).ShouldBeFalse();
        }

        [Fact]
        public void positive_assembly_test()
        {
            var rule = new Subscription(typeof(NewUser).Assembly);

            rule.Matches(typeof(NewUser)).ShouldBeTrue();
            rule.Matches(typeof(EditUser)).ShouldBeTrue();
            rule.Matches(typeof(DeleteUser)).ShouldBeTrue();
        }

        [Fact]
        public void positive_namespace_test()
        {
            var rule = new Subscription
            {
                Scope = RoutingScope.Namespace,
                Match = typeof(RedMessage1).Namespace
            };

            rule.Matches(typeof(RedMessage1)).ShouldBeTrue();
            rule.Matches(typeof(RedMessage2)).ShouldBeTrue();
            rule.Matches(typeof(RedMessage3)).ShouldBeTrue();
        }
    }



}
