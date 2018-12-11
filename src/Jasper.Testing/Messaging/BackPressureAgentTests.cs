using Jasper.Messaging;
using Jasper.Messaging.Transports;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class BackPressureAgentTests
    {
        public BackPressureAgentTests()
        {
            theRoot = new MockMessagingRoot();
            theAgent = new BackPressureAgent(theRoot);
        }

        private readonly MockMessagingRoot theRoot;
        private readonly BackPressureAgent theAgent;

        [Fact]
        public void status_is_accepting_and_above_the_threshold()
        {
            theRoot.Workers.QueuedCount.Returns(theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold + 5);
            theRoot.ListeningStatus = ListeningStatus.Accepting;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.TooBusy);
        }

        [Fact]
        public void status_is_accepting_and_at_the_threshold()
        {
            theRoot.Workers.QueuedCount.Returns(theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold);
            theRoot.ListeningStatus = ListeningStatus.Accepting;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.Accepting);
        }


        [Fact]
        public void status_is_accepting_and_below_the_threshold()
        {
            theRoot.Workers.QueuedCount.Returns(theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold - 5);
            theRoot.ListeningStatus = ListeningStatus.Accepting;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.Accepting);
        }

        [Fact]
        public void status_is_too_busy_and_above_the_threshold()
        {
            theRoot.Workers.QueuedCount.Returns(theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold + 5);
            theRoot.ListeningStatus = ListeningStatus.TooBusy;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.TooBusy);
        }

        [Fact]
        public void status_is_too_busy_and_below_10_percent_of_the_threshold()
        {
            theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold = 2000;
            theRoot.Workers.QueuedCount.Returns(1599);
            theRoot.ListeningStatus = ListeningStatus.TooBusy;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.Accepting);
        }

        [Fact]
        public void status_is_too_busy_and_within_10_percent_of_the_threshold()
        {
            theRoot.Settings.MaximumLocalEnqueuedBackPressureThreshold = 2000;
            theRoot.Workers.QueuedCount.Returns(1801);
            theRoot.ListeningStatus = ListeningStatus.TooBusy;

            theAgent.ApplyBackPressure();

            theRoot.ListeningStatus.ShouldBe(ListeningStatus.TooBusy);
        }
    }
}
