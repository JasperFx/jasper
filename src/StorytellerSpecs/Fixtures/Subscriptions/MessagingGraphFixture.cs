using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    [Hidden]
    public class MessagingGraphFixture : BusFixture
    {
        private MessagingGraph _graph;
        private PublisherSubscriberMismatch _mismatch;

        public MessagingGraphFixture()
        {
            Title = "Messaging Graph State and Validation";
        }

        public override void SetUp()
        {
            _graph = Context.State.Retrieve<MessagingGraph>();
        }

        public IGrammar TheMessageTracksShouldBe()
        {
            return VerifySetOf<MessageRoute>(() => _graph.Matched)
                .Titled("The message routes should be")
                .MatchOn(x => x.MessageType, x => x.Publisher, x => x.Receiver, x => x.ContentType);
        }

        public IGrammar NoSubscribersShouldBe()
        {
            return VerifySetOf<PublishedMessage>(() => _graph.NoSubscribers)
                .Titled("The messages published with no subscribers are")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.ContentTypes);
        }

        public IGrammar NoPublishersShouldBe()
        {
            return VerifySetOf<Subscription>(() => _graph.NoPublishers)
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

        public IGrammar MismatchPropertiesAre()
        {
            return VerifyPropertiesOf<PublisherSubscriberMismatch>("The mismatch properties should be",_ =>
            {
                _.Object = () => _mismatch;
                _.Check(x => x.IncompatibleTransports);
                _.Check(x => x.IncompatibleContentTypes);
            });
        }

        [FormatAs("There are no subscription errors of any kind")]
        public bool NoSubscriptionErrors()
        {
            StoryTellerAssert.Fail(_graph.Mismatches.Any(), () => $"Detected publisher/subscriber mismatches{Environment.NewLine}" + _graph.Mismatches.Select(x => x.ToString()).Join(Environment.NewLine));
            StoryTellerAssert.Fail(_graph.NoPublishers.Any(), () => $"Detected subscriptions with no publishers{Environment.NewLine}" + _graph.NoPublishers.Select(x => x.ToString()).Join(Environment.NewLine));
            StoryTellerAssert.Fail(_graph.NoSubscribers.Any(), () => $"Detected published messages with no subscribers{Environment.NewLine}" + _graph.NoSubscribers.Select(x => x.ToString()).Join(Environment.NewLine));

            return true;
        }


    }
}
