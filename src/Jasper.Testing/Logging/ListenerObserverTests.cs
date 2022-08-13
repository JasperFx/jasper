using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Logging
{
    public class ListenerObserverTests
    {
        private ListenerTracker theTracker = new ListenerTracker(NullLogger.Instance);

        [Fact]
        public void initial_state_by_endpoint_name_is_unknown()
        {
            theTracker.StatusFor("foo")
                .ShouldBe(ListeningStatus.Unknown);
        }

        [Fact]
        public void initial_state_by_uri_is_unknown()
        {
            theTracker.StatusFor(TransportConstants.LocalUri)
                .ShouldBe(ListeningStatus.Unknown);
        }

        [Fact]
        public async Task record_status()
        {
            var waiter = theTracker.WaitForListenerStatus(TransportConstants.LocalUri, ListeningStatus.Accepting,
                10.Seconds());

            theTracker.Publish(new ListenerState(TransportConstants.LocalUri,"DefaultLocal", ListeningStatus.Accepting) );

            await waiter;

            theTracker.StatusFor(TransportConstants.LocalUri)
                .ShouldBe(ListeningStatus.Accepting);
        }

        [Fact]
        public async Task record_status_and_wait_by_endpoint_name()
        {
            var waiter = theTracker.WaitForListenerStatus("local", ListeningStatus.Accepting,
                10.Seconds());

            theTracker.Publish(new ListenerState(TransportConstants.LocalUri,"local", ListeningStatus.Accepting) );

            await waiter;

            theTracker.StatusFor("local")
                .ShouldBe(ListeningStatus.Accepting);
        }
    }
}
