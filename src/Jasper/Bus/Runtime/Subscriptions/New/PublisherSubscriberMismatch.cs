namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class PublisherSubscriberMismatch
    {
        public PublisherSubscriberMismatch(PublishedMessage publisher, NewSubscription subscription)
        {
        }

        public string MessageType { get; }

        public bool IncompatibleTransports { get; }
        public bool IncompatibleContentTypes { get; }

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
