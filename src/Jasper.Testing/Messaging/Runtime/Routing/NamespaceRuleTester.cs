using Jasper.Messaging.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Routing
{
    public class NamespaceRuleTester
    {
        [Fact]
        public void positive_test()
        {
            var rule = NamespaceRule.For<Red.RedMessage1>();
            rule.Matches(typeof(Red.RedMessage1)).ShouldBeTrue();
            rule.Matches(typeof(Red.RedMessage2)).ShouldBeTrue();
            rule.Matches(typeof(Red.RedMessage3)).ShouldBeTrue();
        }

        [Fact]
        public void negative_test()
        {
            var rule = NamespaceRule.For<Red.RedMessage1>();
            rule.Matches(typeof(Green.GreenMessage1)).ShouldBeFalse();
            rule.Matches(typeof(Green.GreenMessage2)).ShouldBeFalse();
            rule.Matches(typeof(Green.GreenMessage3)).ShouldBeFalse();
        }
    }

    namespace Red
    {
        public class RedMessage1{}
        public class RedMessage2{}
        public class RedMessage3{}
    }

    namespace Green
    {
        public class GreenMessage1 { }
        public class GreenMessage2 { }
        public class GreenMessage3 { }
    }
}
