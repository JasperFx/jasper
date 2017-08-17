using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Tracking;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class MessagingGraph
    {
        public MessagingGraph(ServiceCapabilities[] capabilities)
        {

        }

        public MessageRoute[] Matched { get; }

        public PublishedMessage[] NoSubscribers { get; }

        public NewSubscription[] NoPublishers { get; }

        public PublisherSubscriberMismatch[] Mismatches { get; }

    }
}
