using System;
using System.Linq;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Jasper.Http.Testing.AspNetCoreIntegration
{
    public static class ContainerExtensions
    {
        public static IHost DefaultRegistrationIs<T, TConcrete>(this IHost runtime) where TConcrete : T
        {
            runtime.Get<T>().ShouldBeOfType<TConcrete>();
            return runtime;
        }

        public static IHost DefaultRegistrationIs(this IHost runtime, Type pluginType,
            Type concreteType)
        {
            runtime.Get(pluginType).ShouldBeOfType(concreteType);

            return runtime;
        }

        public static IHost DefaultRegistrationIs<T>(this IHost runtime, T value) where T : class
        {
            runtime.Get<T>().ShouldBeSameAs(value);

            return runtime;
        }

        public static IHost DefaultSingletonIs(this IHost runtime, Type pluginType, Type concreteType)
        {
            var @default = runtime.Get<IContainer>().Model.For(pluginType).Default;


            @default.ImplementationType.ShouldBe(concreteType);
            @default.Lifetime.ShouldBe(ServiceLifetime.Singleton);

            return runtime;
        }

        public static IHost DefaultSingletonIs<T, TConcrete>(this IHost container) where TConcrete : T
        {
            return container.DefaultSingletonIs(typeof(T), typeof(TConcrete));
        }

        public static IHost ShouldHaveRegistration<T, TConcrete>(this IHost runtime)
        {
            runtime.Get<IContainer>().Model.For<T>().Instances.Any(x => x.ImplementationType == typeof(TConcrete))
                .ShouldBeTrue();

            return runtime;
        }

        public static IHost ShouldNotHaveRegistration<T, TConcrete>(this IHost runtime)
        {
            runtime.Get<IContainer>().Model.For<T>().Instances.Any(x => x.ImplementationType == typeof(TConcrete))
                .ShouldBeFalse();

            return runtime;
        }
    }
}
