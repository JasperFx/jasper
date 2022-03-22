using System.Threading.Tasks;
using Jasper.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class no_available_route_behavior : IntegrationContext
    {
        [Fact]
        public async Task throw_no_route_exception_by_default()
        {
            await Should.ThrowAsync<NoRoutesException>(async () =>
            {
                await Publisher.SendAsync(new MessageWithNoRoutes());
            });
        }

        public no_available_route_behavior(DefaultApp @default) : base(@default)
        {
        }
    }

    public class MessageWithNoRoutes
    {
    }
}
