using Jasper.Bus.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class registering_services : BootstrappingContext
    {
        [Fact]
        public void channel_graph_is_registered_in_the_container()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(theRuntime.Container.GetInstance<ChannelGraph>());
        }
    }
}