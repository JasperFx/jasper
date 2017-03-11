using Shouldly;
using StructureMap.TypeRules;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    public class can_establish_the_application_assembly_when_bootstrapping_from_basic : IntegrationContext
    {
        [Fact]
        public void can_establish_the_application_assembly()
        {
            withAllDefaults();
            
            Runtime.ApplicationAssembly.ShouldBe(GetType().GetAssembly());
        }
    }
}