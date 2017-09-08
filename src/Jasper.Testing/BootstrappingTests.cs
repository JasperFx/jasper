using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Http;
using Module1;
using Shouldly;
using StructureMap;
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
                _.Services.AddService<IFakeStore, FakeStore>();
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
            theRegistry.Services.AddService<IMainService, MainService>();

            feature1 = theRegistry.Features.For<FakeFeature1>();
            feature1.Services.For<IFeatureService1>().Use<FeatureService1>();

            feature2 = theRegistry.Features.For<FakeFeature2>();
            feature2.Services.For<IFeatureService2>().Use<FeatureService2>();

            feature3 = theRegistry.Features.For<FakeFeature3>();
            feature3.Services.For<IFeatureService3>().Use<FeatureService3>();

            theRegistry.Services.AddService<IFakeStore, FakeStore>();
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
        public void the_container_should_be_locked_for_disposal()
        {
            theRuntime.Container.DisposalLock.ShouldBe(DisposalLock.Ignore);
        }

        [Fact]
        public void registrations_from_the_main_registry_are_applied()
        {
            theRuntime.Container.Model.DefaultTypeFor<IMainService>()
                .ShouldBe(typeof(MainService));
        }

        [Fact]
        public void should_pick_up_registrations_from_the_features()
        {
            theRuntime.Container.Model.DefaultTypeFor<IFeatureService1>()
                .ShouldBe(typeof(FeatureService1));

            theRuntime.Container.Model.DefaultTypeFor<IFeatureService2>()
                .ShouldBe(typeof(FeatureService2));

            theRuntime.Container.Model.DefaultTypeFor<IFeatureService3>()
                .ShouldBe(typeof(FeatureService3));
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
            theRegistry.Services.ForSingletonOf<IMainService>()
                .Use(mainService);

            theRegistry.Services.AddService<IFakeStore, FakeStore>();
            theRegistry.Services.For<IWidget>().Use<Widget>();
            theRegistry.Services.For<IFakeService>().Use<FakeService>();

            feature1 = theRegistry.Features.For<FakeFeature1>();

            feature2 = theRegistry.Features.For<FakeFeature2>();

            feature3 = theRegistry.Features.For<FakeFeature3>();

            theRuntime = JasperRuntime.For(theRegistry);

            theRuntime.Dispose();
        }

        [Fact]
        public void the_container_should_be_disposed()
        {
            mainService.WasDisposed.ShouldBeTrue();

            theRuntime.Container.DisposalLock.ShouldBe(DisposalLock.Unlocked);
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

        public Registry Services { get; } = new Registry();
        public JasperRegistry Registry { get; private set; }

        public Task<Registry> Bootstrap(JasperRegistry registry)
        {
            Registry = registry;
            return Task.FromResult(Services);
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
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
