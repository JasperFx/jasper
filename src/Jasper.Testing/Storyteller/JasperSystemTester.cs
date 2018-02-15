using System;
using System.Threading.Tasks;
using Jasper.Http;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Storyteller;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StoryTeller;
using Xunit;

namespace Jasper.Testing.Storyteller
{
    [Collection("integration")]
    public class JasperSystemTester
    {
        [Fact]
        public async Task bootstraps_the_runtime()
        {
            using (var system = JasperStorytellerHost.Basic(x =>
            {
                x.Handlers.DisableConventionalDiscovery();
                x.Http.Actions.DisableConventionalDiscovery();
            }))
            {
                await system.Warmup();

                ShouldBeNullExtensions.ShouldNotBeNull(system.Runtime.Get<MessagingSettings>());
            }
        }

        [Fact]
        public async Task disposes_the_runtime()
        {
            var system = new FakeStorytellerSystem();

            await system.Warmup();

            system.Dispose();

            system.DisposableGuy.WasDisposed.ShouldBeTrue();
        }

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
        public async Task before_all_is_called_in_warmup_with_runtime()
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
        public async Task context_can_return_services()
        {
            using (var system = new FakeStorytellerSystem())
            {
                await system.Warmup();

                using (var context = system.CreateContext())
                {
                    context.GetService<MessagingSettings>().ShouldBeTheSameAs(system.Runtime.Get<MessagingSettings>());
                }

            }
        }


    }

    public class FakeStorytellerSystem : JasperStorytellerHost<JasperHttpRegistry>
    {
        public readonly DisposableGuy DisposableGuy = new DisposableGuy();

        public FakeStorytellerSystem()
        {
            Registry.Services.AddSingleton(DisposableGuy);
            Registry.Http.Actions.DisableConventionalDiscovery();
            Registry.Handlers.DisableConventionalDiscovery();
        }

        protected override void beforeAll()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(Runtime);
            BeforeAllWasCalled = true;
        }

        public bool BeforeAllWasCalled { get; set; }

        protected override void afterEach(ISpecContext context)
        {
            AfterEachWasCalled = true;
        }

        public bool AfterEachWasCalled { get; set; }

        protected override void beforeEach()
        {
            BeforeEachWasCalled = true;
        }

        public bool BeforeEachWasCalled { get; set; }

        protected override void afterAll()
        {
            AfterAllWasCalled = true;
        }

        public bool AfterAllWasCalled { get; set; }
    }

    public class DisposableGuy : IDisposable
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }
    }
}
