using System.Linq;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    public class can_compile_a_handler_chain_for_an_inner_type : IntegrationContext
    {
        [Fact]
        public void does_not_blow_up()
        {
            withAllDefaults();

            var chain = Handlers.ChainFor<Message1>();
            var call = chain.Handlers.First(x => x.HandlerType == typeof(ThingWithInner.InnerHandler));
            ShouldBeNullExtensions.ShouldNotBeNull(call);
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
