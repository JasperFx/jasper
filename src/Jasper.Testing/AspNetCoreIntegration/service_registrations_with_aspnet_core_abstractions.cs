using System;
using System.Linq;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap;
using StructureMap.Pipeline;
using Xunit;

namespace Jasper.Testing.AspNetCoreIntegration
{
    public class service_registrations_with_aspnet_core_abstractions
    {
        [Fact]
        public void services_registered_by_the_DI_abstraction_are_in_the_container()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();
            registry.Services.AddService<IFakeStore, FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                var @default = runtime.Container.Model.For<IService>().Default;
                @default.ShouldNotBeNull();
                @default.Lifecycle.ShouldBeOfType<UniquePerRequestLifecycle>();
                @default.ReturnedType.ShouldBe(typeof(FooService));
            }
        }

        [Fact]
        public void adds_the_core_service_provider_abstractions()
        {
            var registry = new JasperRegistry();
            registry.Services.AddTransient<IService, FooService>();
            registry.Services.AddService<IFakeStore, FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Container.Model.HasDefaultImplementationFor<IServiceProvider>();
                runtime.Container.Model.HasDefaultImplementationFor<IServiceScopeFactory>();
            }
        }

        public interface IService{}
        public class FooService : IService{}
    }

    public static class ContainerExtensions
    {
        public static IContainer DefaultRegistrationIs<T, TConcrete>(this IContainer container) where TConcrete : T
        {
            container.Model.DefaultTypeFor<T>().ShouldBe(typeof(TConcrete));
            return container;
        }

        public static IContainer DefaultRegistrationIs(this IContainer container, Type pluginType, Type concreteType)
        {
            container.Model.DefaultTypeFor(pluginType).ShouldBe(concreteType);

            return container;
        }

        public static IContainer DefaultRegistrationIs<T>(this IContainer container, T value) where T : class
        {
            container.Model.For<T>().Default.Get<T>().ShouldBeTheSameAs(value);

            return container;
        }

        public static IContainer DefaultSingletonIs(this IContainer container, Type pluginType, Type concreteType)
        {
            container.DefaultRegistrationIs(pluginType, concreteType);
            container.Model.For(pluginType).Default.Lifecycle.ShouldBeOfType<SingletonLifecycle>();

            return container;
        }

        public static IContainer DefaultSingletonIs<T, TConcrete>(this IContainer container) where TConcrete : T
        {
            container.DefaultRegistrationIs<T, TConcrete>();
            container.Model.For<T>().Default.Lifecycle.ShouldBeOfType<SingletonLifecycle>();

            return container;
        }

        public static IContainer ShouldHaveRegistration<T, TConcrete>(this IContainer container)
        {
            var plugin = container.Model.For<T>();
            plugin.Instances.Any(x => x.ReturnedType == typeof(TConcrete)).ShouldBeTrue();

            return container;
        }

        public static IContainer ShouldNotHaveRegistration<T, TConcrete>(this IContainer container)
        {
            var plugin = container.Model.For<T>();
            plugin.Instances.Any(x => x.ReturnedType == typeof(TConcrete)).ShouldBeFalse();

            return container;
        }
    }
}
