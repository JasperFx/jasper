using System;
using System.Linq;
using Jasper;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HttpTests.AspNetCoreIntegration
{
    public static class ContainerExtensions
    {
        public static JasperRuntime DefaultRegistrationIs<T, TConcrete>(this JasperRuntime runtime) where TConcrete : T
        {
            runtime.Get<T>().ShouldBeOfType<TConcrete>();
            return runtime;
        }

        public static JasperRuntime DefaultRegistrationIs(this JasperRuntime runtime, Type pluginType,
            Type concreteType)
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
        }
    }
}
