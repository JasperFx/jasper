using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class determine_global_reply_uri : BootstrappingContext
    {
        [Fact]
        public async Task no_global_subscriber_uri_and_tcp_listener()
        {
            theRegistry.Transports.LightweightListenerAt(2222);

            (await theRuntime()).Get<IChannelGraph>().SystemReplyUri
                .ShouldBe($"tcp://localhost:2222".ToUri().ToMachineUri());
        }

        [Fact]
        public async Task no_global_subscriber_uri_and_durable_tcp_listener()
        {
            theRegistry.Transports.DurableListenerAt(2333);

            (await theRuntime()).Get<IChannelGraph>().SystemReplyUri
                .ShouldBe($"tcp://localhost:2333/durable".ToUri().ToMachineUri());
        }

        [Fact]
        public async Task has_global_subscriber_so_that_wins()
        {
            theRegistry.Transports.DurableListenerAt(2333);
            theRegistry.Subscribe.At("tcp://server1:2345");

            (await theRuntime()).Get<IChannelGraph>().SystemReplyUri
                .ShouldBe("tcp://server1:2345".ToUri().ToMachineUri());
        }
    }
}
