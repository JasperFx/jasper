using System;
using System.Linq;
using Jasper;
using Jasper.Testing;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace IntegrationTests.AspNetCoreIntegration
{
    public class service_registrations_with_aspnet_core_abstractions
    {
        [Fact]
        public void services_registered_by_the_DI_abstraction_are_in_the_container()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {

                var @default = runtime.Services.Last(x => x.ServiceType == typeof(IService));
                SpecificationExtensions.ShouldNotBeNull(@default);
                @default.ImplementationType.ShouldBe(typeof(FooService));
            }
        }

        [Fact]
        public void adds_the_core_service_provider_abstractions()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                SpecificationExtensions.ShouldNotBeNull(runtime.Get<IServiceProvider>());
                SpecificationExtensions.ShouldNotBeNull(runtime.Get<IServiceScopeFactory>());
            }
        }

        public interface IService{}
        public class FooService : IService{}
    }

    public static class ContainerExtensions
    {
        public static JasperRuntime DefaultRegistrationIs<T, TConcrete>(this JasperRuntime runtime) where TConcrete : T
        {
            runtime.Get<T>().ShouldBeOfType<TConcrete>();
            return runtime;

        }

        public static JasperRuntime DefaultRegistrationIs(this JasperRuntime runtime, Type pluginType, Type concreteType)
        {
            runtime.Get(pluginType).ShouldBeOfType(concreteType);

            return runtime;
        }

        public static JasperRuntime DefaultRegistrationIs<T>(this JasperRuntime runtime, T value) where T : class
        {
            runtime.Get<T>().ShouldBeSameAs(value);

            return runtime;
        }

        public static JasperRuntime DefaultSingletonIs(this JasperRuntime runtime, Type pluginType, Type concreteType)
        {
            var @default = runtime.Services.Last(x => x.ServiceType == pluginType);
            @default.ImplementationType.ShouldBe(concreteType);
            @default.Lifetime.ShouldBe(ServiceLifetime.Singleton);

            return runtime;
        }

        public static JasperRuntime DefaultSingletonIs<T, TConcrete>(this JasperRuntime container) where TConcrete : T
        {
            return container.DefaultSingletonIs(typeof(T), typeof(TConcrete));
        }

        public static JasperRuntime ShouldHaveRegistration<T, TConcrete>(this JasperRuntime runtime)
        {
            runtime.Services.Any(x => x.ServiceType == typeof(T) && x.ImplementationType == typeof(TConcrete))
                .ShouldBeTrue();

            return runtime;
        }

        public static JasperRuntime ShouldNotHaveRegistration<T, TConcrete>(this JasperRuntime runtime)
        {
            runtime.Services.Any(x => x.ServiceType == typeof(T) && x.ImplementationType == typeof(TConcrete))
                .ShouldBeFalse();

            return runtime;
        }
    }
}
