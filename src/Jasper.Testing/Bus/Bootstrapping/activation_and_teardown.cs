using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {
        [Fact]
        public void has_service_capabilities()
        {
            theRuntime.Capabilities.ShouldNotBeNull();
        }

        [Fact]
        public void subscriptions_repository_must_be_a_singleton()
        {
            theRuntime.Services.Where(x => x.ServiceType == typeof(ISubscriptionsRepository))
                .All(x => x.Lifetime == ServiceLifetime.Singleton).ShouldBeTrue();
        }

        [Fact]
        public void transport_is_disposed()
        {
            var transport = theRuntime.Get<ITransport[]>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            theRuntime.Dispose();

            transport.WasDisposed.ShouldBeTrue();
        }
    }
}
