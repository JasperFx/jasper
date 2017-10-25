using System.Reflection;
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
    public class JasperRegistryTests
    {
        [Fact]
        public void can_determine_the_root_assembly_on_subclass()
        {
            new MyRegistry().ApplicationAssembly.ShouldBe(typeof(JasperRegistryTests).GetAssembly());
        }

        [Fact]
        public void sets_up_the_container_with_services()
        {
            var registry = new JasperRegistry();
            registry.Services.For<IFoo>().Use<Foo>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Container.DefaultRegistrationIs<IFoo, Foo>();
            }
        }

        public interface IFoo { }
        public class Foo : IFoo { }

        public class MyRegistry : JasperRegistry
        {

        }
    }
}
