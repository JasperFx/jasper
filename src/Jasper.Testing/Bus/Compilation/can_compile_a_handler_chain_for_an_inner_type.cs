using System;
using System.Linq;
using Green;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Compilation
{
    public class can_compile_a_handler_chain_for_an_inner_type : IntegrationContext
    {
        [Fact]
        public void does_not_blow_up()
        {
            withAllDefaults();

            var chain = Graph.ChainFor<Message1>();
            var call = chain.Handlers.First(x => x.HandlerType == typeof(ThingWithInner.InnerHandler));
            call.ShouldNotBeNull();
        }
    }

    public class ThingWithInner
    {
        public class InnerHandler
        {
            public void Go(Message1 message)
            {

            }
        }
    }
}