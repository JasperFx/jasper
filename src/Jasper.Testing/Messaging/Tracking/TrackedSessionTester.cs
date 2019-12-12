using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Tracking
{
    public class TrackedSessionTester
    {
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly TrackedSession theSession = new TrackedSession(null)
        {

        };

        [Fact]
        public async Task throw_if_any_exceptions_happy_path()
        {
            theSession.Record(EventType.Sent, theEnvelope);
            await theSession.Track();
            theSession.AssertNoExceptionsWereThrown();
        }

        [Fact]
        public async Task throw_if_any_exceptions_sad_path()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope);
            theSession.Record(EventType.ExecutionFinished, theEnvelope, ex:new DivideByZeroException());
            await theSession.Track();

            Should.Throw<AggregateException>(() => theSession.AssertNoExceptionsWereThrown());
        }

        [Fact]
        public async Task throw_if_any_exceptions_and_completed_happy_path()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope);
            theSession.Record(EventType.ExecutionFinished, theEnvelope);
            await theSession.Track();
            theSession.AssertNoExceptionsWereThrown();
            theSession.AssertNotTimedOut();
        }

        [Fact]
        public async Task throw_if_any_exceptions_and_completed_sad_path_with_exceptions()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope);
            theSession.Record(EventType.ExecutionFinished, theEnvelope, ex:new DivideByZeroException());
            await theSession.Track();

            Should.Throw<AggregateException>(() =>
            {
                theSession.AssertNoExceptionsWereThrown();
                theSession.AssertNotTimedOut();
            });
        }

        [Fact]
        public async Task throw_if_any_exceptions_and_completed_sad_path_with_never_completed()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope);
            await theSession.Track();

            Should.Throw<TimeoutException>(() =>
            {
                theSession.AssertNoExceptionsWereThrown();
                theSession.AssertNotTimedOut();
            });
        }


    }
}
