using System;
using System.Threading.Tasks;
using Jasper.Testing;
using Jasper.Testing.Bus.Runtime;
using Xunit;

namespace IntegrationTests.Bus
{
    public class defensive_checks_on_request_reply_when_a_transport_has_no_listener : SendingContext
    {
        [Fact]
        public Task throw_invalid_operation_on_request_reply()
        {
            StartTheSender(_ =>
            {
                // No listener on the lightweight
                _.Publish.Message<Message1>().To("tcp://localhost:2222");
            });

            return Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () =>
            {
                await theSender.Bus.Request<Message2>(new Message1());
            });
        }

        [Fact]
        public Task throw_invalid_operation_on_send_and_await()
        {
            StartTheSender(_ =>
            {
                // No listener on the lightweight
                _.Publish.Message<Message1>().To("tcp://localhost:2277");
            });

            return Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () =>
            {
                await theSender.Bus.SendAndWait(new Message1());
            });
        }
    }
}
