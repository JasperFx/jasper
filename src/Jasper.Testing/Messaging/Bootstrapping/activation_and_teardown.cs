using System.Linq;
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
        public void has_service_capabilities()
        {
            theRuntime.Capabilities.ShouldNotBeNull();
        }

        [Fact]
        public void subscriptions_repository_must_be_a_singleton()
        {
            theRuntime.Container.Model.For<ISubscriptionsRepository>().Default.Lifetime.ShouldBe(ServiceLifetime.Singleton);
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
