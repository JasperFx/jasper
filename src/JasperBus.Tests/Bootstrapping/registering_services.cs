using JasperBus.Configuration;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    public class registering_services : BootstrappingContext
    {
        [Fact]
        public void channel_graph_is_registered_in_the_container()
        {
            theRuntime.Container.GetInstance<ChannelGraph>()
                .ShouldNotBeNull();
        }
    }
}