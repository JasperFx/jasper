using System.Reflection;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class JasperRegistryTests
    {
        [Fact]
        public void can_determine_the_root_assembly_on_subclass()
        {
            new MyRegistry().ApplicationAssembly.ShouldBe(typeof(JasperRegistryTests).GetTypeInfo().Assembly);
        }

        public class MyRegistry : JasperRegistry
        {
            
        }
    }
}