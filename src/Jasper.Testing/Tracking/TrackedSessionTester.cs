using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Testing.Messaging;
using Jasper.Tracking;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Tracking
{
    public class TrackedSessionTester : IDisposable
    {
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly TrackedSession theSession;
        private readonly IHost _host;


        public TrackedSessionTester()
        {
            _host = JasperHost.For(x => x.UseMessageTrackingTestingSupport());

            _host.Services.GetRequiredService<IMessageLogger>()
                .ShouldBeOfType<MessageTrackingLogger>();

            theSession = new TrackedSession(_host);
        }

        [Fact]
        public async Task throw_if_any_exceptions_happy_path()
        {
            theSession.Record(EventType.Sent, theEnvelope, "", 1);
            await theSession.Track();
            theSession.AssertNoExceptionsWereThrown();
        }

        [Fact]
        public async Task throw_if_any_exceptions_sad_path()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope, "", 1);
            theSession.Record(EventType.ExecutionFinished, theEnvelope, "", 1, ex: new DivideByZeroException());
            await theSession.Track();

            Should.Throw<AggregateException>(() => theSession.AssertNoExceptionsWereThrown());
        }

        [Fact]
        public async Task throw_if_any_exceptions_and_completed_happy_path()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope, "", 1);
            theSession.Record(EventType.ExecutionFinished, theEnvelope, "", 1);
            await theSession.Track();
            theSession.AssertNoExceptionsWereThrown();
            theSession.AssertNotTimedOut();
        }

        [Fact]
        public async Task throw_if_any_exceptions_and_completed_sad_path_with_exceptions()
        {
            theSession.Record(EventType.ExecutionStarted, theEnvelope, "", 1);
            theSession.Record(EventType.ExecutionFinished, theEnvelope, "", 1, ex: new DivideByZeroException());
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
            theSession.Record(EventType.ExecutionStarted, theEnvelope, "", 1);
            await theSession.Track();

            Should.Throw<TimeoutException>(() =>
            {
                theSession.AssertNoExceptionsWereThrown();
                theSession.AssertNotTimedOut();
            });
        }


        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
