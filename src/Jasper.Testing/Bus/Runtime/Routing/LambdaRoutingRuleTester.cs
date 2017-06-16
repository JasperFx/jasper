using Jasper.Bus.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Routing
{
    public class LambdaRoutingRuleTester
    {
        [Fact]
        public void positive_match()
        {
            var rule = new LambdaRoutingRule("is type",type => type == typeof (BusSettings));
            ShouldBeBooleanExtensions.ShouldBeTrue(rule.Matches(typeof(BusSettings)));
        }

        [Fact]
        public void negative_match()
        {
            var rule = new LambdaRoutingRule("test",type => type == typeof(BusSettings));
            ShouldBeBooleanExtensions.ShouldBeFalse(rule.Matches(GetType()));
        }
    }
}