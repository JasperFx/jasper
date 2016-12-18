using Jasper.Codegen;
using Jasper.Testing.Codegen.IoC;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class InjectedFieldTests
    {
        [Fact]
        public void injected_field_says_that_is_injected()
        {
            new InjectedField(typeof(ITouchService))
                .Creation.ShouldBe(VariableCreation.Injected);
        }
    }
}