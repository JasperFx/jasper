using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Shouldly;
using TestingSupport;
using Xunit;

namespace MessagingTests.Tracking
{
    public class MessageHistoryTester
    {
        public MessageHistoryTester()
        {
            watch = history.Watch(() => { });
        }

        private readonly MessageHistory history = new MessageHistory();
        private readonly Task<MessageTrack[]> watch;


        [Fact]
        public void assert_exceptions()
        {
            // Nothing happens
            history.AssertNoExceptions();

            history.LogException(new DivideByZeroException());

            Should.Throw<AggregateException>(() => history.AssertNoExceptions())
                .InnerExceptions.Single().ShouldBeOfType<DivideByZeroException>();
        }

        [Fact]
        public void complete_with_exception()
        {
            var ex = new DivideByZeroException();

            var envelope1 = ObjectMother.Envelope();
            history.Start(envelope1, "Envelope");
            history.Complete(envelope1, "Envelope", ex);

            watch.IsCompleted.ShouldBeTrue();

            watch.Result.Single().ExceptionText.ShouldBe(ex.ToString());
        }

        [Fact]
        public void completes_only_when_all_outstanding_work_is_completed()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();

            history.Start(envelope1, "Envelope");

            watch.IsCompleted.ShouldBeFalse();

            history.Start(envelope2, "Envelope");

            watch.IsCompleted.ShouldBeFalse();

            history.Complete(envelope1, "Envelope");

            watch.IsCompleted.ShouldBeFalse();

            history.Complete(envelope2, "Envelope");

            watch.IsCompleted.ShouldBeTrue();

            var tracks = watch.Result;

            foreach (var messageTrack in tracks)
            {
                messageTrack.Completed.ShouldBeTrue();
                ShouldBeNullExtensions.ShouldNotBeNull(messageTrack.Headers);
                ShouldBeNullExtensions.ShouldBeNull(messageTrack.ExceptionText);
            }
        }


        [Fact]
        public void knows_when_stuff_is_finished_starting_from_scratch()
        {
            watch.IsCompleted.ShouldBeFalse();
        }
    }

    public class end_to_end_watching_with_a_failure
    {
        [Fact]
        public async Task history_can_still_do_the_watch()
        {
            using (var runtime = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<MessageThatFailsHandler>();
                _.Include<MessageTrackingExtension>();
                _.Publish.AllMessagesTo(TransportConstants.LoopbackUri);
            }))
            {
                var history = runtime.Get<MessageHistory>();

                await history.WatchAsync(() => runtime.Messaging.Send(new MessageThatFails()));

                var aggregate = Exception<AggregateException>.ShouldBeThrownBy(() => history.AssertNoExceptions());
                aggregate
                    .InnerExceptions.Single().ShouldBeOfType<DivideByZeroException>();
            }
        }
    }

    public class MessageThatFails
    {
    }

    [JasperIgnore]
    public class MessageThatFailsHandler
    {
        public void Handle(MessageThatFails message)
        {
            throw new DivideByZeroException();
        }
    }
}
