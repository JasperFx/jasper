using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;
using Jasper.Bus.Transports.Lightweight;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.Lightweight
{
    public class PortListenerTests
    {
        private readonly IHandlerPipeline thePipeline
            = Substitute.For<IHandlerPipeline>();

        private readonly ChannelGraph theChannels = new ChannelGraph();
        private PortListener theListener;

        public PortListenerTests()
        {
            theListener = new PortListener(2222, Substitute.For<IInMemoryQueue>(), Substitute.For<IBusLogger>());
            theListener.AddQueue("one", thePipeline, theChannels, new ChannelNode("jasper://localhost:2222/one".ToUri()));
            theListener.AddQueue("two", thePipeline, theChannels, new ChannelNode("jasper://localhost:2222/two".ToUri()));
        }

        [Fact]
        public void succeed_when_all_of_the_queues_are_known()
        {
            var envelopes = new Envelope[]
            {
                new Envelope {Queue = "one"},
                new Envelope {Queue = "one"},
                new Envelope {Queue = "one"},
                new Envelope {Queue = "two"},
                new Envelope {Queue = "two"},
            };

            theListener.Received(envelopes)
                .ShouldBe(ReceivedStatus.Successful);


        }


        [Fact]
        public void do_nothing_when_queue_does_not_exist    ()
        {
            var envelopes = new Envelope[]
            {
                new Envelope {Queue = "one"},
                new Envelope {Queue = "one"},
                new Envelope {Queue = "one"},
                new Envelope {Queue = "two"},
                new Envelope {Queue = "three"},
            };

            theListener.Received(envelopes)
                .ShouldBe(ReceivedStatus.QueueDoesNotExist);


        }
    }
}
