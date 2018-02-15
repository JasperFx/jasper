using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Configuration;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Bootstrapping;
using Jasper.Testing.Messaging.Compilation;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing
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
        public void registrations_from_the_main_registry_are_applied()
        {
            using (var runtime = JasperRuntime.For(_ => { _.Services.AddTransient<IMainService, MainService>(); }))
            {
                runtime.Container.DefaultRegistrationIs<IMainService, MainService>();
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
    }

    public class when_bootstrapping_a_runtime_with_multiple_features : IDisposable
    {
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

        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;




    }

    public class when_shutting_down_the_runtime
    {
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

        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;
        private readonly MainService mainService = new MainService();

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
        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public bool WasStarted { get; set; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }

        public bool WasStopped { get; set; }
    }


}
