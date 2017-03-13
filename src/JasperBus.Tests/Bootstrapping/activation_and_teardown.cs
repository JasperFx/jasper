using System.Linq;
using JasperBus.Runtime;
using JasperBus.Tests.Stubs;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {
        [Fact]
        public void transport_is_disposed()
        {
            var transport = theRuntime.Container.GetAllInstances<ITransport>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            theRuntime.Dispose();

            transport.WasDisposed.ShouldBeTrue();
        }
    }
}