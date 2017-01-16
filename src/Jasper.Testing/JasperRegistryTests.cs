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
        public void can_explicitly_define_the_application_assembly()
        {
            var registry = new JasperRegistry();
            registry.ApplicationAssembly.ShouldBeNull();

            registry.ApplicationContains<MyRegistry>();

            registry.ApplicationAssembly.ShouldBe(typeof(JasperRegistryTests).GetAssembly());
        }

        public class MyRegistry : JasperRegistry
        {
            
        }
    }
}