using System.Linq;
using System.Threading.Tasks;
using Jasper.Testing.Messaging.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public class can_compile_a_handler_chain_for_an_inner_type : IntegrationContext
    {
        [Fact]
        public async Task does_not_blow_up()
        {
            await withAllDefaults();

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
