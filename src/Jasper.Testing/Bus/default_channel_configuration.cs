using System.Linq;
using Jasper.Bus;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class default_channel_configuration
    {
        [Fact]
        public void use_the_loopback_replies_queue_by_default()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Messaging.Handlers.ConventionalDiscoveryDisabled = true;
            }))
            {

                var channels = runtime.Get<ChannelGraph>();
                channels.DefaultChannel
                    .ShouldBeTheSameAs(channels.IncomingChannelsFor("loopback").Single());
            }
        }

        [Fact]
        public void override_the_default_channel()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Messaging.Handlers.ConventionalDiscoveryDisabled = true;
                _.Channels.DefaultIs("loopback://incoming");
            }))
            {
                var channels = runtime.Get<ChannelGraph>();
                channels.DefaultChannel
                    .ShouldBeTheSameAs(channels["loopback://incoming"]);
            }
        }
    }
}
