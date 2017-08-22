namespace Jasper.Bus.Runtime.Subscriptions
{
    public class PublisherSubscriberMismatch
    {
        public PublisherSubscriberMismatch(PublishedMessage publisher, Subscription subscription)
        {
            MessageType = subscription.MessageType;
            Publisher = publisher.ServiceName;
            Subscriber = subscription.ServiceName;
            SubscriberTransport = subscription.Destination.Scheme;
            PublishedContentTypes = publisher.ContentTypes;
            SubscriberContentTypes = subscription.Accept;
        }

        public string MessageType { get; }

        public bool IncompatibleTransports { get; internal set; }
        public bool IncompatibleContentTypes { get; internal set; }

        public string[] PublisherTransports { get; } = new string[0];
        public string SubscriberTransport { get; }
        public string[] PublishedContentTypes { get; }
        public string[] SubscriberContentTypes { get; }

        public string Publisher { get; }
        public string Subscriber { get; }

        public override string ToString()
        {
            return $"{nameof(MessageType)}: {MessageType}, {nameof(Publisher)}: {Publisher}, {nameof(Subscriber)}: {Subscriber}";
        }
    }
}
