using System;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Testing.Bus.Runtime;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class no_avalailable_route_behavior : IntegrationContext
    {
        [Fact]
        public async Task throw_no_route_exception_by_default()
        {
            withAllDefaults();

            await Exception<NoRoutesException>.ShouldBeThrownBy(async () =>
            {
                await Bus.Send(new MessageWithNoRoutes());
            });
        }

        [Fact]
        public async Task do_not_throw_when_configured_to_ignore()
        {
            with(_ =>
            {
                _.Advanced.NoMessageRouteBehavior = NoRouteBehavior.Ignore;
            });

            await Bus.Send(new MessageWithNoRoutes());
        }
    }

    public class MessageWithNoRoutes
    {

    }
}
