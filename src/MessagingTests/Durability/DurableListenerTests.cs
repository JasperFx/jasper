using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using NSubstitute;
using Xunit;

namespace MessagingTests.Durability
{
    public class DurableListenerTests
    {
        [Fact]
        public void status_delegates_to_agent()
        {
            var agent = Substitute.For<IListeningAgent>();
            var listener = new DurableListener(agent, null, null, null, null, null);

            listener.Status = ListeningStatus.Accepting;
            agent.Received().Status = ListeningStatus.Accepting;
        }

        [Fact]
        public void status_delegates_to_agent_2()
        {
            var agent = Substitute.For<IListeningAgent>();
            var listener = new DurableListener(agent, null, null, null, null, null);

            listener.Status = ListeningStatus.TooBusy;
            agent.Received().Status = ListeningStatus.TooBusy;
        }
    }
}
