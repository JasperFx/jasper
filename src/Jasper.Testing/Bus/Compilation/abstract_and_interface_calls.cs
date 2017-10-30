using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    [Collection("compilation")]
    public class abstract_and_interface_calls : CompilationContext
    {
        public abstract_and_interface_calls()
        {
            theRegistry.Handlers.IncludeType<HandlerWithMultipleCalls>();
        }

        [Fact]
        public async Task can_collate_interfaces()
        {
            var message = new SpecificMessage();

            await Execute(message);

            HandlerWithMultipleCalls.LastIMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task can_collate_abstract_types()
        {
            var message = new SpecificMessage2();

            await Execute(message);

            HandlerWithMultipleCalls.LastIMessage.ShouldBeSameAs(message);
            HandlerWithMultipleCalls.LastBaseMessage.ShouldBeSameAs(message);
        }
    }

    public class HandlerWithMultipleCalls
    {
        public void Handle(SpecificMessage message)
        {

        }

        public void Handle(SpecificMessage2 message2)
        {

        }

        public void Handle(IMessage message)
        {
            LastIMessage = message;
        }

        public void Handle(BaseMessage message)
        {
            LastBaseMessage = message;
        }

        public static IMessage LastIMessage { get; set; }
        public static BaseMessage LastBaseMessage { get; set; }
    }

    public interface IMessage
    {

    }

    public class SpecificMessage : IMessage
    {

    }

    public abstract class BaseMessage : IMessage
    {

    }

    public class SpecificMessage2 : BaseMessage
    {

    }
}
