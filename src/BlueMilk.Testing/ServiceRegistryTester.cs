using System.Linq;
using Jasper.Internals;
using Jasper.Testing.Internals.TargetTypes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals
{
    public class ServiceRegistryTester
    {
        [Fact]
        public void for_use()
        {
            var registry = new ServiceRegistry();
            registry.For<IWidget>().Use<AWidget>();

            var descriptor = registry.Single();

            descriptor.ImplementationType.ShouldBe(typeof(AWidget));
            descriptor.ServiceType.ShouldBe(typeof(IWidget));
            descriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);

        }

        [Fact]
        public void forsingleton_use()
        {
            var registry = new ServiceRegistry();
            registry.ForSingletonOf<IWidget>().Use<AWidget>();

            var descriptor = registry.Single();

            descriptor.ImplementationType.ShouldBe(typeof(AWidget));
            descriptor.ServiceType.ShouldBe(typeof(IWidget));
            descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }
    }
}
