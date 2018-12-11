using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Testing.Messaging.Compilation;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class can_customize_handler_chain_through_Configure_call_on_HandlerType : IntegrationContext
    {
        [Fact]
        public async Task the_configure_method_is_found_and_used()
        {
            await withAllDefaults();

            chainFor<SpecialMessage>().ShouldBeWrappedWith<FakeFrame>();
        }
    }

    public class SpecialMessage
    {
    }

    public class CustomizedHandler
    {
        public void Handle(SpecialMessage message)
        {
        }

        public static void Configure(HandlerChain chain)
        {
            chain.Middleware.Add(new FakeFrame());
        }
    }
}
