using BlueMilk.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals.Codegen
{
    public class ReflectionExtensionsTests
    {
        [Fact]
        public void get_full_name_in_code_for_generic_type()
        {
            typeof(Handler<Message1>).FullNameInCode()
                .ShouldBe($"Jasper.Testing.Internals.Codegen.Handler<{typeof(Message1).FullName}>");
        }
    }

    public class Handler<T>
    {

    }

    public class Message1{}
}
