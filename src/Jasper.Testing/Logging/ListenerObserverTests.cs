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
        private ListenerObserver theObserver = new ListenerObserver(NullLogger.Instance);

        [Fact]
        public void initial_state_by_endpoint_name_is_unknown()
        {
            theObserver.StatusFor("foo")
                .ShouldBe(ListeningStatus.Unknown);
        }

        [Fact]
        public void initial_state_by_uri_is_unknown()
        {
            theObserver.StatusFor(TransportConstants.LocalUri)
                .ShouldBe(ListeningStatus.Unknown);
        }

        [Fact]
        public async Task record_status()
        {
            var waiter = theObserver.WaitForListenerStatus(TransportConstants.LocalUri, ListeningStatus.Accepting,
                10.Seconds());

            theObserver.Publish(new ListenerState(TransportConstants.LocalUri,"DefaultLocal", ListeningStatus.Accepting) );

            await waiter;

            theObserver.StatusFor(TransportConstants.LocalUri)
                .ShouldBe(ListeningStatus.Accepting);
        }

        [Fact]
        public async Task record_status_and_wait_by_endpoint_name()
        {
            var waiter = theObserver.WaitForListenerStatus("local", ListeningStatus.Accepting,
                10.Seconds());

            theObserver.Publish(new ListenerState(TransportConstants.LocalUri,"local", ListeningStatus.Accepting) );

            await waiter;

            theObserver.StatusFor("local")
                .ShouldBe(ListeningStatus.Accepting);
        }
    }
}
