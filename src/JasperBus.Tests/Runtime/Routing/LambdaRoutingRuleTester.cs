using JasperBus.Runtime.Routing;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Routing
{
    public class LambdaRoutingRuleTester
    {
        [Fact]
        public void positive_match()
        {
            var rule = new LambdaRoutingRule(type => type == typeof (BusSettings));
            rule.Matches(typeof(BusSettings)).ShouldBeTrue();
        }

        [Fact]
        public void negative_match()
        {
            var rule = new LambdaRoutingRule(type => type == typeof(BusSettings));
            rule.Matches(GetType()).ShouldBeFalse();
        }
    }
}