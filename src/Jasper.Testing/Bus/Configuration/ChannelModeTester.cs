using JasperBus.Configuration;
using JasperBus.Runtime;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Configuration
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