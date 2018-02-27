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
        private void start(string urlListener)
        {
            StartTheReceiver(_ =>
            {
                _.Http.Transport.EnableListening(true);

                _.Http
                    .UseUrls(urlListener);

                _.Handlers.IncludeType<MessageConsumer>();
                _.Handlers.IncludeType<RequestReplyHandler>();
            });
        }

        [Fact]
        public void read_by_localhost()
        {
            start("http://localhost:5504");
        }

        [Fact]
        public void read_empty_ip()
        {
            start("http://0.0.0.0:5005");
        }

        [Fact]
        public void can_read_wild_card_uri()
        {
            "http://*:5006".ToUri().Port.ShouldBe(5006);
        }

        [Fact]
        public void read_wildcard()
        {
            start("http://*:5006");
        }
    }
}
