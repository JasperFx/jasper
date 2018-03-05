using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Bootstrapping;
using Jasper.Testing.Messaging.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    [Collection("integration")]
    public class BootstrappingTests
    {
        [Fact]
        public void can_determine_the_application_assembly()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Services.AddTransient<IFakeStore, FakeStore>();
                _.Services.For<IWidget>().Use<Widget>();
                _.Services.For<IFakeService>().Use<FakeService>();
            }))
            {
                runtime.ApplicationAssembly.ShouldBe(GetType().Assembly);
            }
        }

        [Fact]
        public void can_use_custom_hosted_service_without_aspnet()
        {
            var service = new CustomHostedService();

            var runtime = JasperRuntime.For(_ => _.Services.AddSingleton<IHostedService>(service));

            service.WasStarted.ShouldBeTrue();
            service.WasStopped.ShouldBeFalse();

            runtime.Dispose();

            service.WasStopped.ShouldBeTrue();
        }

        [Fact]
        public void registrations_from_the_main_registry_are_applied()
        {
            using (var runtime = JasperRuntime.For(_ => { _.Services.AddTransient<IMainService, MainService>(); }))
            {
                runtime.Container.DefaultRegistrationIs<IMainService, MainService>();
            }
        }
    }

    public class when_bootstrapping_a_runtime_with_multiple_features : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;

        public when_bootstrapping_a_runtime_with_multiple_features()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();

            theRegistry.Services.AddTransient<IMainService, MainService>();

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();
            theRegistry.Services.For<IWidget>().Use<Widget>();
            theRegistry.Services.For<IFakeService>().Use<FakeService>();

            theRuntime = JasperRuntime.For(theRegistry);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }
    }

    public class when_shutting_down_the_runtime
    {
        private readonly MainService mainService = new MainService();

        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;

        public when_shutting_down_the_runtime()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
            theRegistry.Services.AddSingleton<IMainService>(mainService);

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();
            theRegistry.Services.For<IWidget>().Use<Widget>();
            theRegistry.Services.For<IFakeService>().Use<FakeService>();


            theRuntime = JasperRuntime.For(theRegistry);

            theRuntime.Dispose();
        }
    }


    public interface IMainService : IDisposable
    {
    }

    public class MainService : IMainService
    {
        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class CustomHostedService : IHostedService
    {
        public bool WasStarted { get; set; }

        public bool WasStopped { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }
    }
}
