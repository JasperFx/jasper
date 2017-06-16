using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Model
{
    public class MessageHandlerVariableSourceTester
    {
        [Fact]
        public void responds_to_envelope()
        {
            var source = new MessageHandlerVariableSource(typeof(Message1));

            ShouldBeBooleanExtensions.ShouldBeTrue(source.Matches(typeof(Envelope)));

            var variable = source.Create(typeof(Envelope));

            variable.VariableType.ShouldBe(typeof(Envelope));

            variable.Usage.ShouldBe("context.Envelope");
        }

        [Fact]
        public void responds_to_the_message()
        {
            var source = new MessageHandlerVariableSource(typeof(Message1));

            ShouldBeBooleanExtensions.ShouldBeTrue(source.Matches(typeof(Message1)));

            var variable = source.Create(typeof(Message1));

            variable.VariableType.ShouldBe(typeof(Message1));

            variable.Usage.ShouldBe("message1");
        }
    }
}