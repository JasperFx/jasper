using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.AspNetCoreIntegration
{
    public class service_registrations_with_aspnet_core_abstractions
    {
        [Fact]
        public async Task services_registered_by_the_DI_abstraction_are_in_the_container()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();

            var runtime = await JasperRuntime.ForAsync(registry);

            try
            {
                runtime.Container.Model.For<IService>().Default.ImplementationType
                    .ShouldBe(typeof(FooService));
            }
            finally
            {
                await runtime.Shutdown();
            }

        }

        [Fact]
        public async Task adds_the_core_service_provider_abstractions()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();

            var runtime = await JasperRuntime.ForAsync(registry);
            try
            {
                ShouldBeNullExtensions.ShouldNotBeNull(runtime.Get<IServiceProvider>());
                ShouldBeNullExtensions.ShouldNotBeNull(runtime.Get<IServiceScopeFactory>());
            }
            finally
            {
                await runtime.Shutdown();
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
            var @default = runtime.Container.Model.For(pluginType).Default;


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
            runtime.Container.Model.For<T>().Instances.Any(x => x.ImplementationType == typeof(TConcrete))
                .ShouldBeTrue();

            return runtime;
        }

        public static JasperRuntime ShouldNotHaveRegistration<T, TConcrete>(this JasperRuntime runtime)
        {
            runtime.Container.Model.For<T>().Instances.Any(x => x.ImplementationType == typeof(TConcrete))
                .ShouldBeFalse();

            return runtime;

            return runtime;
        }
    }
}
