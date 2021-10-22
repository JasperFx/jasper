using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using Jasper.Transports;
using LamarCodeGeneration.Util;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class ScheduledRetryContinuationTester
    {
        [Fact]
        public async Task do_as_a_delay_w_the_timespan_given()
        {
            var continuation = new ScheduledRetryContinuation(5.Minutes());
            var callback = Substitute.For<IChannelCallback>();

            var envelope = ObjectMother.Envelope();

            var now = DateTime.Today.ToUniversalTime();

            var context = Substitute.For<IExecutionContext>();
            context.Persistence.Returns(Substitute.For<IEnvelopePersistence>());
            context.Envelope.Returns(envelope);
            await continuation.Execute(callback, context, now);


            envelope.ExecutionTime.ShouldBe(now.AddMinutes(5));

            await context.Persistence.Received().ScheduleJob(envelope);
        }

        [Fact]
        public async Task do_as_a_delay_w_the_timespan_given_with_native_rescheduling()
        {
            var continuation = new ScheduledRetryContinuation(5.Minutes());
            var callback = Substitute.For<IChannelCallback, IHasNativeScheduling>();

            var envelope = ObjectMother.Envelope();

            var now = DateTime.Today.ToUniversalTime();
            var context = Substitute.For<IExecutionContext>();
            context.Envelope.Returns(envelope);

            var root = new MockMessagingRoot();
            await continuation.Execute(callback, context, now);

            envelope.ExecutionTime.ShouldBe(now.AddMinutes(5));

            await root.Persistence.DidNotReceive().ScheduleJob(envelope);

            await ((IHasNativeScheduling) callback).Received()
                .MoveToScheduledUntil(envelope, envelope.ExecutionTime.Value);
        }
    }
}
