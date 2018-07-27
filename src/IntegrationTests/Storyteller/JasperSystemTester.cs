using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Storyteller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Shouldly;
using StoryTeller;
using Xunit;

namespace IntegrationTests.Storyteller
{
    [Collection("integration")]
    public class JasperSystemTester
    {
        [Fact]
        public async Task adds_console_and_debug_logging()
        {
            using (var system = JasperStorytellerHost.Basic(x =>
            {
                x.HttpRoutes.DisableConventionalDiscovery();
            }))
            {
                await system.Warmup();

                var providerTypes = system
                    .Runtime
                    .Container
                    .Model
                    .For<ILoggerProvider>()
                    .Instances
                    .Select(x => x.ImplementationType)
                    .ToArray();

                providerTypes.ShouldContain(typeof(ConsoleLoggerProvider));
                providerTypes.ShouldContain(typeof(DebugLoggerProvider));
            }
        }


        [Fact]
        public async Task bootstraps_the_runtime()
        {
            using (var system = JasperStorytellerHost.Basic(x =>
            {
                x.HttpRoutes.DisableConventionalDiscovery();
            }))
            {
                await system.Warmup();

                system.Runtime.Get<MessagingSettings>().ShouldNotBeNull();
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
                    context.GetService<MessagingSettings>().ShouldBeSameAs(system.Runtime.Get<MessagingSettings>());
                }

            }
        }


    }

    public class FakeStorytellerSystem : JasperStorytellerHost<JasperRegistry>
    {
        public readonly DisposableGuy DisposableGuy = new DisposableGuy();

        public FakeStorytellerSystem()
        {
            Registry.Services.AddSingleton(DisposableGuy);
            Registry.HttpRoutes.DisableConventionalDiscovery();
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
