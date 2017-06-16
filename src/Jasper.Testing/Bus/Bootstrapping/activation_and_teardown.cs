using System.Linq;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Runtime.Serializers;
using JasperBus.Runtime.Subscriptions;
using JasperBus.Tests.Stubs;
using Shouldly;
using StructureMap.Pipeline;
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

        [Fact]
        public void transports_must_be_a_singleton()
        {
            theRuntime.Container.Model.For<ITransport>().Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        [Fact]
        public void subscriptions_cache_must_be_a_singleton()
        {
            theRuntime.Container.Model.For<ISubscriptionsCache>().Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        [Fact]
        public void subscriptions_repository_must_be_a_singleton()
        {
            theRuntime.Container.Model.For<ISubscriptionsRepository>().Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        [Fact]
        public void subscriptions_storage_must_be_a_singleton()
        {
            theRuntime.Container.Model.For<ISubscriptionsStorage>().Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        [Fact]
        public void should_have_the_envelope_sender_registered()
        {
            theRuntime.Container.DefaultRegistrationIs<IEnvelopeSender, EnvelopeSender>();
        }

        [Fact]
        public void should_have_the_service_bus_registered()
        {
            theRuntime.Container.DefaultRegistrationIs<IServiceBus, ServiceBus>();
        }

        [Fact]
        public void should_have_the_envelope_serializer_registered()
        {
            theRuntime.Container.DefaultSingletonIs<IEnvelopeSerializer, EnvelopeSerializer>();
        }

        [Fact]
        public void should_have_the_handler_pipeline_registered()
        {
            theRuntime.Container.DefaultRegistrationIs<IHandlerPipeline, HandlerPipeline>();
        }
    }
}
