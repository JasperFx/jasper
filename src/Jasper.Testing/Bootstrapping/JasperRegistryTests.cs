using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class JasperRegistryTests
    {
        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        public class MyRegistry : JasperRegistry
        {
        }

        [Fact]
        public void can_determine_the_root_assembly_on_subclass()
        {
            new MyRegistry().ApplicationAssembly.ShouldBe(typeof(JasperRegistryTests).Assembly);
        }

        [Fact]
        public void sets_up_the_container_with_services()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Services.For<IFoo>().Use<Foo>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();

            using (var runtime = JasperHost.For(registry))
            {
                runtime.Container.DefaultRegistrationIs<IFoo, Foo>();
            }
        }
    }
}
