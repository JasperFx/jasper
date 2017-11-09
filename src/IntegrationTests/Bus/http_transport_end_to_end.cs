using System;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using IntegrationTests.Conneg;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Testing.Bus.Lightweight;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
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

                _.Handlers.IncludeType<MessageConsumer>();
                _.Handlers.IncludeType<RequestReplyHandler>();
            });


        }

        [Fact]
        public void http_transport_is_enabled_and_registered()
        {
            var busSettings = theReceiver.Get<BusSettings>();
            busSettings.Http.EnableMessageTransport.ShouldBeTrue();

            theReceiver.Get<IChannelGraph>().ValidTransports.ShouldContain("http");
        }

        [Fact]
        public void send_messages_end_to_end_lightweight()
        {
            StartTheSender(_ =>
            {
                _.Publish.Message<Message1>().To("http://localhost:5002/messages");
                _.Publish.Message<Message2>().To("http://localhost:5002/messages");
            });

            var waiter1 = theTracker.WaitFor<Message1>();
            var waiter2 = theTracker.WaitFor<Message2>();

            var message1 = new Message1 {Id = Guid.NewGuid()};
            var message2 = new Message2 {Id = Guid.NewGuid()};

            theSender.Bus.Send(message1);
            theSender.Bus.Send(message2);

            waiter1.Wait(10.Seconds());
            waiter2.Wait(10.Seconds());

            waiter1.Result.Message.As<Message1>()
                .Id.ShouldBe(message1.Id);
        }

    }


}
