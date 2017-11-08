using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace IntegrationTests.Bus
{
    public class http_transport_end_to_end : SendingContext
    {
        public http_transport_end_to_end()
        {
            StartTheReceiver(_ =>
            {
                _.Transports.Http.Enable(true);

                _.Http
                    .UseUrls("http://localhost:5002")
                    .UseKestrel();
            });
        }

        [Fact]
        public void http_transport_is_enabled_and_registered()
        {
            var busSettings = theReceiver.Get<BusSettings>();
            busSettings.Http.EnableMessageTransport.ShouldBeTrue();

            theReceiver.Get<IChannelGraph>().ValidTransports.ShouldContain("http");
        }
    }


}
