using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Jasper.Testing.Bus.Bootstrapping;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.TypeRules;
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
                runtime.ApplicationAssembly.ShouldBe(GetType().GetAssembly());
            }
        }
    }

    public class when_bootstrapping_a_runtime_with_multiple_features : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private FakeFeature1 feature1;
        private FakeFeature2 feature2;
        private FakeFeature3 feature3;
        private JasperRuntime theRuntime;

        public when_bootstrapping_a_runtime_with_multiple_features()
        {
            theRegistry.Services.AddTransient<IMainService, MainService>();

            feature1 = theRegistry.Features.For<FakeFeature1>();
            feature1.Services.For<IFeatureService1>().Use<FeatureService1>();

            feature2 = theRegistry.Features.For<FakeFeature2>();
            feature2.Services.For<IFeatureService2>().Use<FeatureService2>();

            feature3 = theRegistry.Features.For<FakeFeature3>();
            feature3.Services.For<IFeatureService3>().Use<FeatureService3>();

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();
            theRegistry.Services.For<IWidget>().Use<Widget>();
            theRegistry.Services.For<IFakeService>().Use<FakeService>();

            theRuntime = JasperRuntime.For(theRegistry);
        }

        [Fact]
        public void all_features_are_bootstrapped()
        {
            feature1.Registry.ShouldBeSameAs(theRegistry);
            feature2.Registry.ShouldBeSameAs(theRegistry);
            feature3.Registry.ShouldBeSameAs(theRegistry);
        }

        [Fact]
        public void each_feature_is_activated()
        {
            feature1.WasActivated.ShouldBeTrue();
            feature2.WasActivated.ShouldBeTrue();
            feature3.WasActivated.ShouldBeTrue();
        }

        [Fact]
        public void registrations_from_the_main_registry_are_applied()
        {
            theRuntime.Container.DefaultRegistrationIs<IMainService, MainService>();
        }

        [Fact]
        public void should_pick_up_registrations_from_the_features()
        {
            theRuntime.Container.DefaultRegistrationIs<IFeatureService1, FeatureService1>();
            theRuntime.Container.DefaultRegistrationIs<IFeatureService2, FeatureService2>();
            theRuntime.Container.DefaultRegistrationIs<IFeatureService3, FeatureService3>();
        }

        public void Dispose()
        {
            feature1?.Dispose();
            feature2?.Dispose();
            feature3?.Dispose();
            theRuntime?.Dispose();
        }
    }

    public class when_shutting_down_the_runtime
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private FakeFeature1 feature1;
        private FakeFeature2 feature2;
        private FakeFeature3 feature3;
        private JasperRuntime theRuntime;
        private MainService mainService = new MainService();



        public when_shutting_down_the_runtime()
        {
            theRegistry.Services.AddSingleton<IMainService>(mainService);

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();
            theRegistry.Services.For<IWidget>().Use<Widget>();
            theRegistry.Services.For<IFakeService>().Use<FakeService>();

            feature1 = theRegistry.Features.For<FakeFeature1>();

            feature2 = theRegistry.Features.For<FakeFeature2>();

            feature3 = theRegistry.Features.For<FakeFeature3>();

            theRuntime = JasperRuntime.For(theRegistry);

            theRuntime.Dispose();
        }

        [Fact]
        public void each_feature_should_be_disposed()
        {
            feature1.WasDisposed.ShouldBeTrue();
            feature2.WasDisposed.ShouldBeTrue();
            feature3.WasDisposed.ShouldBeTrue();
        }




    }

    public class FakeFeature1 : IFeature
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public ServiceRegistry Services { get; } = new ServiceRegistry();
        public JasperRegistry Registry { get; private set; }

        public Task<ServiceRegistry> Bootstrap(JasperRegistry registry)
        {
            Registry = registry;
            return Task.FromResult(Services);
        }

        public Task Activate(JasperRuntime runtime, GenerationRules generation)
        {
            Runtime = runtime;
            WasActivated = true;
            return Task.CompletedTask;
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            throw new NotSupportedException();
        }

        public JasperRuntime Runtime { get; set; }

        public bool WasActivated { get; set; }
    }

    public interface IMainService : IDisposable{}

    public class MainService : IMainService
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }
    }

    public interface IFeatureService1{}
    public class FeatureService1 : IFeatureService1{}

    public interface IFeatureService2{}
    public class FeatureService2 : IFeatureService2{}

    public interface IFeatureService3{}
    public class FeatureService3 : IFeatureService3{}


    public class FakeFeature2 : FakeFeature1{}
    public class FakeFeature3 : FakeFeature1{}
}
