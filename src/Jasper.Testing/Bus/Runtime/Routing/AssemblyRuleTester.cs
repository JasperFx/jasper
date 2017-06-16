using Jasper.Bus.Runtime.Routing;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Routing
{
    public class AssemblyRuleTester
    {
        [Fact]
        public void positive_test()
        {
            var rule = AssemblyRule.For<NewUser>();
            ShouldBeBooleanExtensions.ShouldBeTrue(rule.Matches(typeof(NewUser)));
            ShouldBeBooleanExtensions.ShouldBeTrue(rule.Matches(typeof(EditUser)));
            ShouldBeBooleanExtensions.ShouldBeTrue(rule.Matches(typeof(DeleteUser)));
        }

        [Fact]
        public void negative_test()
        {
            var rule = AssemblyRule.For<NewUser>();
            ShouldBeBooleanExtensions.ShouldBeFalse(rule.Matches(typeof(Message1)));
            ShouldBeBooleanExtensions.ShouldBeFalse(rule.Matches(typeof(Message2)));
            ShouldBeBooleanExtensions.ShouldBeFalse(rule.Matches(GetType()));
        }
    }

}