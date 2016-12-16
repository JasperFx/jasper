using Jasper.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class HandlerGenerationTests: CompilationContext
    {
        private AsyncMode theAsyncMode => new HandlerGeneration(theChain, new GenerationConfig("Something"), "input")
            .AsyncMode;

        [Fact]
        public void detects_that_it_is_all_sync()
        {
            theChain.Call<MainInput>(x => x.DoSync());

            theAsyncMode.ShouldBe(AsyncMode.ReturnCompletedTask);

            theChain.Call<MainInput>(x => x.Touch());

            theAsyncMode.ShouldBe(AsyncMode.ReturnCompletedTask);
        }

        [Fact]
        public void detects_chain_as_async_task_if_multiple_asyncs()
        {
            theChain.Call<MainInput>(x => x.DoSync());
            theChain.Call<MainInput>(x => x.TouchAsync());
            theChain.Call<MainInput>(x => x.DifferentAsync());

            theAsyncMode.ShouldBe(AsyncMode.AsyncTask);
        }

        [Fact]
        public void use_async_task_if_one_async_that_is_not_last()
        {
            theChain.Call<MainInput>(x => x.DoSync());
            theChain.Call<MainInput>(x => x.TouchAsync());

            theAsyncMode.ShouldBe(AsyncMode.ReturnFromLastNode);
        }
    }
}