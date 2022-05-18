using Jasper.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime.Routing
{
    public class NoNamedEndpointRouteTests
    {
        [Fact]
        public void throws_descriptive_exception()
        {
            var route = new NoNamedEndpointRoute("foo", new[] { "bar", "baz" });

            var ex = Should.Throw<UnknownEndpointException>(() => route.CreateForSending(null, null, null, null));

            ex.Message.ShouldBe("Endpoint name 'foo' is invalid. Known endpoints are bar, baz");
        }
    }
}
