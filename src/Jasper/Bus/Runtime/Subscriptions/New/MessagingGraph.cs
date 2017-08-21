using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Tracking;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class MessagingGraph
    {
        public MessagingGraph(ServiceCapabilities[] capabilities)
        {
            NoPublishers = new NewSubscription[0];
            NoSubscribers = new PublishedMessage[0];
            Matched = new MessageRoute[0];
            Mismatches = new PublisherSubscriberMismatch[0];

        }

        public MessageRoute[] Matched { get; }

        public PublishedMessage[] NoSubscribers { get; }

        public NewSubscription[] NoPublishers { get; }

        public PublisherSubscriberMismatch[] Mismatches { get; }

    }
}
