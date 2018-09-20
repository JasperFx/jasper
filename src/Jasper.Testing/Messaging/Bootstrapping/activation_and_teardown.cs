using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {

        [Fact]
        public async Task transport_is_disposed()
        {
            var runtime = await theRuntime();
            var transport = runtime.Get<ITransport[]>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            await runtime.Shutdown();

            transport.WasDisposed.ShouldBeTrue();
        }
    }
}
