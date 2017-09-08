using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class default_retry_channel_configuration : BootstrappingContext
    {
        [Fact]
        public void should_use_the_loopback_retries_queue_by_default()
        {
            theChannels.DefaultRetryChannel.Destination.ShouldBe("loopback://retries".ToUri());
        }


    }
}
