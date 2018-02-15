using Jasper.Messaging.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Routing
{
    public class LambdaRoutingRuleTester
    {
        [Fact]
        public void positive_match()
        {
            var rule = new LambdaRoutingRule("is type",type => type == typeof (FakeAppSettings));
            ShouldBeBooleanExtensions.ShouldBeTrue(rule.Matches(typeof(FakeAppSettings)));
        }

        [Fact]
        public void negative_match()
        {
            var rule = new LambdaRoutingRule("test",type => type == typeof(FakeAppSettings));
            ShouldBeBooleanExtensions.ShouldBeFalse(rule.Matches(GetType()));
        }
    }
}