using System;
using System.IO;
using System.Threading.Tasks;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Configuration;
using Jasper.Testing.Bus.Bootstrapping;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
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


        [Fact]
        public void registrations_from_the_main_registry_are_applied()
        {
            theRuntime.Container.DefaultRegistrationIs<IMainService, MainService>();
        }

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


}
