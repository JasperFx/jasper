using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class ScheduledRetryContinuationTester
    {
        [Fact]
        public async Task do_as_a_delay_w_the_timespan_given()
        {
            var continuation = new ScheduledRetryContinuation(5.Minutes());
            var context = Substitute.For<IMessageContext>();


            var envelope = ObjectMother.Envelope();
            envelope.Callback = Substitute.For<IMessageCallback>();

            context.Envelope.Returns(envelope);

            var now = DateTime.Today.ToUniversalTime();

            await continuation.Execute(context, now);


#pragma warning disable 4014
            envelope.Callback.Received().MoveToScheduledUntil(now.AddMinutes(5), envelope);
#pragma warning restore 4014
        }
    }
}
