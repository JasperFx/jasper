using System;
using System.Threading.Tasks;
using Jasper.Testing.Messaging.Runtime;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class defensive_checks_on_request_reply_when_a_transport_has_no_listener : SendingContext
    {
        [Fact]
        public async Task throw_invalid_operation_on_request_reply()
        {
            await StartTheSender(_ =>
            {
                // No listener on the lightweight
                _.Publish.Message<Message1>().To("tcp://localhost:2222");
            });

            await Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () =>
            {
                await theSender.Messaging.Request<Message2>(new Message1());
            });
        }

        [Fact]
        public async Task throw_invalid_operation_on_send_and_await()
        {
            await StartTheSender(_ =>
            {
                // No listener on the lightweight
                _.Publish.Message<Message1>().To("tcp://localhost:2277");
            });

            await Exception<InvalidOperationException>.ShouldBeThrownByAsync(async () =>
            {
                await theSender.Messaging.SendAndWait(new Message1());
            });
        }
    }
}
