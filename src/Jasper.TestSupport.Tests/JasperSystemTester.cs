using System;
using System.Threading.Tasks;
using Jasper.TestSupport.Storyteller;
using JasperHttp;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StoryTeller;
using Xunit;

namespace Jasper.TestSupport.Tests
{
    [Collection("integration")]
    public class JasperSystemTester
    {

        [Fact]
        public async Task after_all_is_called_in_dispose()
        {
            var system = new FakeStorytellerSystem();

            await system.Warmup();

            system.AfterAllWasCalled.ShouldBeFalse();
            system.Dispose();

            system.AfterAllWasCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task after_each_is_called_on_context_after_execution()
        {
            using (var system = new FakeStorytellerSystem())
            {
                await system.Warmup();

                system.AfterEachWasCalled.ShouldBeFalse();
                using (var context = system.CreateContext())
                {
                    var specContext = SpecContext.ForTesting();
                    context.BeforeExecution(specContext);

                    context.AfterExecution(specContext);
                    system.AfterEachWasCalled.ShouldBeTrue();
                }
            }
        }

        [Fact]
        public async Task before_all_is_called_in_warmup_with_host()
        {
            using (var system = new FakeStorytellerSystem())
            {
                system.BeforeAllWasCalled.ShouldBeFalse();
                await system.Warmup();

                system.BeforeAllWasCalled.ShouldBeTrue();
            }
        }

        [Fact]
        public async Task before_each_is_called_on_context_creation()
        {
            using (var system = new FakeStorytellerSystem())
            {
                await system.Warmup();

                system.BeforeEachWasCalled.ShouldBeFalse();
                using (var context = system.CreateContext())
                {
                    system.BeforeEachWasCalled.ShouldBeTrue();
                }
            }
        }


        [Fact]
        public async Task bootstraps_the_host()
        {
            using (var system = JasperStorytellerHost.Basic(x => { x.Http(opts => opts.DisableConventionalDiscovery());; }))
            {
                await system.Warmup();

                system.Runtime.Get<JasperOptions>().ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task context_can_return_services()
        {
            using (var system = new FakeStorytellerSystem())
            {
                await system.Warmup();

                using (var context = system.CreateContext())
                {
                    context.GetService<JasperOptions>().ShouldBeSameAs(system.Runtime.Get<JasperOptions>());
                }
            }
        }

        [Fact]
        public async Task disposes_the_host()
        {
            var system = new FakeStorytellerSystem();

            await system.Warmup();

            system.Dispose();

            system.DisposableGuy.WasDisposed.ShouldBeTrue();
        }
    }

    public class FakeStorytellerSystem : JasperStorytellerHost<JasperRegistry>
    {
        public readonly DisposableGuy DisposableGuy = new DisposableGuy();

        public FakeStorytellerSystem()
        {
            Registry.Services.AddSingleton(DisposableGuy);
            Registry.Http(opts => opts.DisableConventionalDiscovery());;
        }

        public bool BeforeAllWasCalled { get; set; }

        public bool AfterEachWasCalled { get; set; }

        public bool BeforeEachWasCalled { get; set; }

        public bool AfterAllWasCalled { get; set; }

        protected override void beforeAll()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(Runtime);
            BeforeAllWasCalled = true;
        }

        protected override void afterEach(ISpecContext context)
        {
            AfterEachWasCalled = true;
        }

        protected override void beforeEach()
        {
            BeforeEachWasCalled = true;
        }

        protected override void afterAll()
        {
            AfterAllWasCalled = true;
        }
    }

    public class DisposableGuy : IDisposable
    {
        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }
}
