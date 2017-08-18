using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions.New;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    [Hidden]
    public class MessagingGraphFixture : Fixture
    {
        private MessagingGraph _graph;
        private PublisherSubscriberMismatch _mismatch;

        public override void SetUp()
        {
            var list = Context.State.Retrieve<IList<ServiceCapabilities>>();
            _graph = new MessagingGraph(list.ToArray());
        }

        public IGrammar TheMessageTracksShouldBe()
        {
            return VerifySetOf<MessageRoute>(() => _graph.Matched)
                .Titled("The message routes should be")
                .MatchOn(x => x.MessageType, x => x.Destination, x => x.Publisher, x => x.ContentType);
        }

        public IGrammar NoSubscribersShouldBe()
        {
            return VerifySetOf<PublishedMessage>(() => _graph.NoSubscribers)
                .Titled("The messages published with no subscribers are")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.ContentTypes);
        }

        public IGrammar NoPublishersShouldBe()
        {
            return VerifySetOf<NewSubscription>(() => _graph.NoPublishers)
                .Titled("The subscriptions with no publishers are")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.Destination);
        }

        public IGrammar MismatchesAre()
        {
            return VerifySetOf<PublisherSubscriberMismatch>(() => _graph.Mismatches)
                .Titled("The detected mismatches between subscribers and publishers are")
                .MatchOn(x => x.MessageType, x => x.Publisher, x => x.Subscriber);
        }

        [FormatAs("For detected mismatch for {messageType} from {publisher} to {subscriber}")]
        public void ForMismatch([SelectionList("MessageTypes")]string messageType, string publisher, string subscriber)
        {
            _mismatch = _graph.Mismatches.FirstOrDefault(x =>
                x.MessageType == messageType && x.Publisher == publisher && x.Subscriber == subscriber);

            StoryTellerAssert.Fail(_mismatch == null, () => $@"Could not find this mismatch, the known mismatches are:
{_graph.Mismatches.Select(x => $"* {x}{Environment.NewLine}")}

");
        }


    }
}