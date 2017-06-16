using Jasper.Bus.Model;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class can_customize_handler_chain_through_Configure_call_on_HandlerType : IntegrationContext
    {
        [Fact]
        public void the_configure_method_is_found_and_used()
        {
            withAllDefaults();

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