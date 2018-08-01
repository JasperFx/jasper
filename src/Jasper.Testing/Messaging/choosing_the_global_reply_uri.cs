using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class choosing_the_global_reply_uri : IntegrationContext
    {
        // Need to test this w/ Http transport enabled and not enabled

        [Fact]
        public async Task use_http_for_the_global_reply_uri_if_it_exists()
        {
            await with(r =>
            {
                r.Hosting.UseUrls("http://*:5066");
                r.Transports.Http.EnableListening(true);
            });

            Runtime.Get<IChannelGraph>().SystemReplyUri.ShouldBe($"http://localhost:5066/messages".ToUri().ToMachineUri());
        }

        [Fact]
        public async Task use_tcp_for_the_global_reply_uri_if_it_exists()
        {
            await with(r =>
            {


                r.Hosting.UseUrls("http://*:5066");

                r.Transports.LightweightListenerAt(4356);

                r.Transports.Http.EnableListening(false);
            });

            Runtime.Get<IChannelGraph>().SystemReplyUri.ShouldBe($"tcp://localhost:4356".ToUri().ToMachineUri());
        }
    }
}
