using System;
using System.Threading.Tasks;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace JasperBus.Tests.Compilation
{
    public class simple_async_message_handlers : CompilationContext<AsyncHandler>
    {
        private ITestOutputHelper _output;

        public simple_async_message_handlers(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
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
    }

    public class AsyncHandler
    {
        public static Message1 LastMessage1;
        public static Message2 LastMessage2;

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
    }



}