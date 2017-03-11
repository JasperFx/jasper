using System.Reflection;
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

            var runtime = JasperRuntime.For(registry);

            runtime.Container.Model.DefaultTypeFor<IFoo>().ShouldBe(typeof(Foo));
        }

        public interface IFoo { }
        public class Foo : IFoo { }

        public class MyRegistry : JasperRegistry
        {
            
        }
    }
}