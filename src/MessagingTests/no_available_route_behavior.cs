using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Shouldly;
using TestingSupport;
using Xunit;

namespace MessagingTests
{
    public class no_available_route_behavior : IntegrationContext
    {
        [Fact]
        public async Task throw_no_route_exception_by_default()
        {
            await Should.ThrowAsync<NoRoutesException>(async () =>
            {
                await Bus.Send(new MessageWithNoRoutes());
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
