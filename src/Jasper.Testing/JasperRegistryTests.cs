using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Bootstrapping;
using Jasper.Testing.Messaging.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    [Collection("integration")]
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
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Container.DefaultRegistrationIs<IFoo, Foo>();
            }
        }
    }
}
