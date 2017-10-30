using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    [Collection("compilation")]
    public class simple_async_message_handlers : CompilationContext
    {
        public simple_async_message_handlers()
        {
            theRegistry.Handlers.IncludeType<AsyncHandler>();
        }

        [Fact]
        public async Task execute_the_simplest_possible_static_chain()
        {
            var message = new Message1();
            await Execute(message);

            AsyncHandler.LastMessage1.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task execute_the_simplest_possible_instance_chain()
        {
            var message = new Message2();
            await Execute(message);

            AsyncHandler.LastMessage2.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task can_pass_in_the_envelope()
        {
            var message = new Message3();
            await Execute(message);

            AsyncHandler.LastEnvelope.ShouldBeSameAs(theEnvelope);
        }

        [Fact]
        public async Task can_pass_in_the_invocation_context()
        {
            var message = new Message4();
            var context = await Execute(message);

            AsyncHandler.LastContext.ShouldBeSameAs(context);
        }

        [Fact]
        public Task can_run_double_async_actions()
        {
            return Execute(new DoubleAction());
        }
    }

    public class AsyncHandler
    {
        public static Message1 LastMessage1;
        public static Message2 LastMessage2;
        public static Envelope LastEnvelope;
        public static IInvocationContext LastContext;

        public static Task Simple1(Message1 message)
        {
            LastMessage1 = message;
            return Task.CompletedTask;
        }

        public Task Simple2(Message2 message)
        {
            LastMessage2 = message;
            return Task.CompletedTask;
        }

        public Task Simple3(Message3 message, Envelope envelope)
        {
            LastEnvelope = envelope;
            return Task.CompletedTask;
        }

        public static Task Simple4(Message4 message, IInvocationContext context)
        {
            LastContext = context;
            return Task.CompletedTask;
        }

        public static Task Handle(DoubleAction action)
        {
            return Task.CompletedTask;
        }

        public static Task Handle(IDoubleAction action)
        {
            return Task.CompletedTask;
        }
    }

    public class DoubleAction : IDoubleAction
    {

    }

    public interface IDoubleAction
    {

    }
}
