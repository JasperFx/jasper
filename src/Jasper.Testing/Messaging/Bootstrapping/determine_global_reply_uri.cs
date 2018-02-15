using System;
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
        public void no_global_subscriber_uri_and_tcp_listener()
        {
            theRegistry.Transports.LightweightListenerAt(2222);

            theRuntime.Get<IChannelGraph>().SystemReplyUri
                .ShouldBe($"tcp://{Environment.MachineName}:2222".ToUri());
        }

        [Fact]
        public void no_global_subscriber_uri_and_durable_tcp_listener()
        {
            theRegistry.Transports.DurableListenerAt(2333);

            theRuntime.Get<IChannelGraph>().SystemReplyUri
                .ShouldBe($"tcp://{Environment.MachineName}:2333/durable".ToUri());
        }

        [Fact]
        public void has_global_subscriber_so_that_wins()
        {
            theRegistry.Transports.DurableListenerAt(2333);
            theRegistry.Subscribe.At("tcp://server1:2345");

            theRuntime.Get<IChannelGraph>().SystemReplyUri
                .ShouldBe("tcp://server1:2345".ToUri());
        }
    }
}
