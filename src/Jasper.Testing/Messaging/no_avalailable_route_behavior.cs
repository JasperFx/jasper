using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class no_avalailable_route_behavior : IntegrationContext
    {
        [Fact]
        public async Task throw_no_route_exception_by_default()
        {
            await withAllDefaults();

            await Exception<NoRoutesException>.ShouldBeThrownByAsync(async () =>
            {
                await Bus.Send(new MessageWithNoRoutes());
            });
        }
    }

    public class MessageWithNoRoutes
    {
    }
}
