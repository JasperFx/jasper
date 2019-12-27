using System.Linq;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public class can_compile_a_handler_chain_for_an_inner_type : Runtime.IntegrationContext
    {
        public can_compile_a_handler_chain_for_an_inner_type(Runtime.DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void does_not_blow_up()
        {
            var chain = Handlers.ChainFor<Message1>();
            var call = chain.Handlers.First(x => x.HandlerType == typeof(ThingWithInner.InnerHandler));
            ShouldBeNullExtensions.ShouldNotBeNull(call);
        }
    }

    public class ThingWithInner
    {
        public class InnerHandler
        {
            public void Handle(Message1 message)
            {
            }
        }
    }
}
