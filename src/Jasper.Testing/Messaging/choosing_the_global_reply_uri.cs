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
        [Fact]
        public async Task use_tcp_for_the_global_reply_uri_if_it_exists()
        {
            await with(r =>
            {


                r.Hosting.UseUrls("http://*:5066");

                r.Transports.LightweightListenerAt(4356);

            });

            Runtime.Get<IChannelGraph>().SystemReplyUri.ShouldBe($"tcp://localhost:4356".ToUri().ToMachineUri());
        }
    }
}
