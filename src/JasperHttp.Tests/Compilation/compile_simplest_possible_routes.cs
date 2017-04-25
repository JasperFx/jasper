using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace JasperHttp.Tests.Compilation
{
    public class compile_simplest_possible_routes : CompilationContext<SimpleRouteHandler>
    {
        [Fact]
        public async Task execute_simplest_possible_sync_route()
        {
            SimpleRouteHandler.GoWasCalled = false;

            await Execute(x => x.Go());

            SimpleRouteHandler.GoWasCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task execute_simplest_possible_async_route()
        {
            SimpleRouteHandler.GoAsyncWasCalled = false;

            await Execute(x => x.GoAsync());

            SimpleRouteHandler.GoAsyncWasCalled.ShouldBeTrue();
        }
    }

    public class SimpleRouteHandler
    {
        public void Go()
        {
            GoWasCalled = true;
        }

        public Task GoAsync()
        {
            GoAsyncWasCalled = true;
            return Task.CompletedTask;
        }

        public static bool GoWasCalled { get; set; }
        public static bool GoAsyncWasCalled { get; set; }
    }

}
