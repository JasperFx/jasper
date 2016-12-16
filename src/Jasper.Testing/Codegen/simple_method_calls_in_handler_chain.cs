using System.Reflection;
using System.Threading.Tasks;
using Jasper.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class simple_method_calls_in_handler_chain : CompilationContext
    {
        [Fact]
        public async Task generate_with_async_handler()
        {
            theChain.Call<MainInput>(x => x.TouchAsync());

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();

            theGeneratedCode.ShouldContain("public Task Handle(");
        }

        [Fact]
        public async Task generate_with_multiple_async_handler()
        {
            theChain.Call<MainInput>(x => x.TouchAsync());
            theChain.Call<MainInput>(x => x.DifferentAsync());

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();

            theGeneratedCode.ShouldContain("public async Task Handle(");
        }

        [Fact]
        public async Task generate_with_sync_handler()
        {
            theChain.Call<MainInput>(x => x.Touch());

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();

            theGeneratedCode.ShouldContain("return Task.CompletedTask;");
        }


        [Fact]
        public async Task generate_with_mixed_sync_and_async_handler()
        {
            theChain.Call<MainInput>(x => x.DifferentAsync());
            theChain.Call<MainInput>(x => x.Touch());

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();
            input.DifferentWasCalled.ShouldBeTrue();
        }

        public static void TouchInput(MainInput input)
        {
            input.Touch();
        }

        [Fact]
        public async Task generate_with_sync_static_method()
        {
            var method = GetType().GetMethod(nameof(TouchInput), BindingFlags.Public | BindingFlags.Static);

            theChain.AddToEnd(new MethodCall(GetType(), method));

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();
        }

        public static void TouchInputAsync(MainInput input)
        {
            input.Touch();
        }

        [Fact]
        public async Task generate_with_async_static_method()
        {
            var method = GetType().GetMethod(nameof(TouchInputAsync), BindingFlags.Public | BindingFlags.Static);

            theChain.AddToEnd(new MethodCall(GetType(), method));

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();
        }
    }
}