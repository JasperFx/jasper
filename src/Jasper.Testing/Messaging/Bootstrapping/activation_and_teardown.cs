using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
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
        public async Task has_service_capabilities()
        {
            (await theRuntime()).Capabilities.ShouldNotBeNull();
        }

        [Fact]
        public async Task subscriptions_repository_must_be_a_singleton()
        {
            (await theRuntime()).Container.Model.For<ISubscriptionsRepository>().Default.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

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
