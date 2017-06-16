using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Configuration
{
    public class ChannelModeTester
    {
        [Fact]
        public void the_default_mode_is_delivery_guaranteed()
        {
            new ChannelNode("stub://one".ToUri())
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);
        }
    }
}