using JasperBus.Runtime.Routing;
using Shouldly;
using TestMessages;
using Xunit;

namespace JasperBus.Tests.Runtime.Routing
{
    public class AssemblyRuleTester
    {
        [Fact]
        public void positive_test()
        {
            var rule = AssemblyRule.For<NewUser>();
            rule.Matches(typeof(NewUser)).ShouldBeTrue();
            rule.Matches(typeof(EditUser)).ShouldBeTrue();
            rule.Matches(typeof(DeleteUser)).ShouldBeTrue();
        }

        [Fact]
        public void negative_test()
        {
            var rule = AssemblyRule.For<NewUser>();
            rule.Matches(typeof(Message1)).ShouldBeFalse();
            rule.Matches(typeof(Message2)).ShouldBeFalse();
            rule.Matches(GetType()).ShouldBeFalse();
        }
    }

}