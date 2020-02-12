using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.ErrorHandling;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using Jasper.Transports;
using LamarCodeGeneration.Util;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class ScheduledRetryContinuationTester
    {
        [Fact]
        public async Task do_as_a_delay_w_the_timespan_given()
        {
            var continuation = new ScheduledRetryContinuation(5.Minutes());
            var context = Substitute.For<IMessageContext>();


            var envelope = ObjectMother.Envelope();
            envelope.Callback = Substitute.For<IFullMessageCallback>();

            context.Envelope.Returns(envelope);

            var now = DateTime.Today.ToUniversalTime();

            var theRoot = new MockMessagingRoot();
            await continuation.Execute(theRoot, context, now);


#pragma warning disable 4014
            envelope.Callback.As<IFullMessageCallback>().Received().MoveToScheduledUntil(now.AddMinutes(5));
#pragma warning restore 4014
        }
    }
}
