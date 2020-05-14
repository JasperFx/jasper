using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing
{
    public class chain_should_flush_messages
    {
        [Fact]
        public void do_flush_if_method_uses_IMessageContext()
        {
            RouteChain.For<SomeEndpointGuy>(x => x.post_do_stuff_with_messages(null, null))
                .ShouldFlushOutgoingMessages()
                .ShouldBeTrue();
        }

        [Fact]
        public void do_not_flush_if_no_usage_of_imessage_context()
        {
            RouteChain.For<SomeEndpointGuy>(x => x.post_do_nothing(null))
                .ShouldFlushOutgoingMessages()
                .ShouldBeFalse();
        }
    }

    public class SomeEndpointGuy
    {
        [HttpPost("/dostuff")]
        public void post_do_stuff_with_messages(IMessageContext context, HttpContext http)
        {
        }

        [HttpPost("/donothing")]
        public void post_do_nothing(HttpContext context)
        {
        }
    }
}
