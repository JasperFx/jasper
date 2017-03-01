using System.Threading.Tasks;
using Module1;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class BootstrappingTests
    {
        //[Fact] -- come back to this later
        public void can_discover_modules_from_assembly_scanning_and_apply_extensions()
        {
            Module1.Module1Extension.Registry = null;

            var registry = new JasperRegistry();
            var runtime = JasperRuntime.For(registry);

            runtime.ShouldNotBeNull();

            Module1Extension.Registry.ShouldBe(registry);
        }
    }
}