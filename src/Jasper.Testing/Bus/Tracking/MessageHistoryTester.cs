using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Tracking;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Tracking
{
    public class MessageHistoryTester
    {
        private readonly MessageHistory history = new MessageHistory();
        private readonly Task<MessageTrack[]> watch;

        public MessageHistoryTester()
        {

            watch = history.Watch(() =>
            {

            });
        }

        [Fact]
        public void knows_when_stuff_is_finished_starting_from_scratch()
        {
            ShouldBeBooleanExtensions.ShouldBeFalse(watch.IsCompleted);
        }

        [Fact]
        public void completes_only_when_all_outstanding_work_is_completed()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();

            history.Start(envelope1, "Envelope");

            ShouldBeBooleanExtensions.ShouldBeFalse(watch.IsCompleted);

            history.Start(envelope2, "Envelope");

            ShouldBeBooleanExtensions.ShouldBeFalse(watch.IsCompleted);

            history.Complete(envelope1, "Envelope");

            ShouldBeBooleanExtensions.ShouldBeFalse(watch.IsCompleted);

            history.Complete(envelope2, "Envelope");

            ShouldBeBooleanExtensions.ShouldBeTrue(watch.IsCompleted);

            var tracks = watch.Result;

            foreach (var messageTrack in tracks)
            {
                ShouldBeBooleanExtensions.ShouldBeTrue(messageTrack.Completed);
                ShouldBeNullExtensions.ShouldNotBeNull(messageTrack.Headers);
                ShouldBeNullExtensions.ShouldBeNull(messageTrack.ExceptionText);
            }
        }

        [Fact]
        public void complete_with_exception()
        {
            var ex = new DivideByZeroException();

            var envelope1 = ObjectMother.Envelope();
            history.Start(envelope1, "Envelope");
            history.Complete(envelope1, "Envelope", ex);

            ShouldBeBooleanExtensions.ShouldBeTrue(watch.IsCompleted);

            watch.Result.Single().ExceptionText.ShouldBe(ex.ToString());

        }
    }
}