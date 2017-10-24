﻿using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Testing.Bus.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Pipeline;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {
        [Fact]
        public void transport_is_disposed()
        {
            var transport = theRuntime.Get<ITransport[]>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            theRuntime.Dispose();

            transport.WasDisposed.ShouldBeTrue();
        }

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
        public void should_have_the_handler_pipeline_registered()
        {
            theRuntime.Container.DefaultRegistrationIs<IHandlerPipeline, HandlerPipeline>();
        }
    }
}
