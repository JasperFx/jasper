using System.Threading.Tasks;
using Jasper.Testing;
using Jasper.Testing.Messaging.Lightweight;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Transport
{
    public class better_reading_of_url_configuration : SendingContext
    {
        private Task start(string urlListener)
        {
            return StartTheReceiver(_ =>
            {
                _.Transports.Http.EnableListening(true);

                _.Hosting
                    .UseUrls(urlListener);

                _.Handlers.IncludeType<MessageConsumer>();
                _.Handlers.IncludeType<RequestReplyHandler>();
            });
        }

        [Fact]
        public Task read_by_localhost()
        {
            return start("http://localhost:5504");
        }

        [Fact]
        public Task read_empty_ip()
        {
            return start("http://0.0.0.0:5005");
        }

        [Fact]
        public void can_read_wild_card_uri()
        {
            "http://*:5006".ToUri().Port.ShouldBe(5006);
        }

        [Fact]
        public Task read_wildcard()
        {
            return start("http://*:5006");
        }
    }
}
