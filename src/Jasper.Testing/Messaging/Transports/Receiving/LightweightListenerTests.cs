using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Receiving
{
    public class LightweightListenerTests
    {
        [Fact]
        public void status_delegates_to_agent()
        {
            var agent = Substitute.For<IListeningAgent>();
            var listener = new LightweightListener(null, null, agent);

            listener.Status = ListeningStatus.Accepting;
            agent.Received().Status = ListeningStatus.Accepting;
        }

        [Fact]
        public void status_delegates_to_agent_2()
        {
            var agent = Substitute.For<IListeningAgent>();
            var listener = new LightweightListener(null, null, agent);

            listener.Status = ListeningStatus.TooBusy;
            agent.Received().Status = ListeningStatus.TooBusy;
        }
    }
}
