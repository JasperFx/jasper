namespace Jasper.Messaging.Runtime
{
    public partial class Envelope
    {
        private const string SentAttemptsHeaderKey = "sent-attempts";
        public const string OriginalIdKey = "original-id";
        public const string SagaIdKey = "saga-id";
        private const string IdKey = "id";
        public const string ParentIdKey = "parent-id";
        private const string ContentTypeKey = "content-type";
        private const string SourceKey = "source";
        public const string ReplyRequestedKey = "reply-requested";
        public const string DestinationKey = "destination";
        public const string ReplyUriKey = "reply-uri";
        public const string ExecutionTimeKey = "time-to-send";
        public const string ReceivedAtKey = "received-at";
        public const string AttemptsKey = "attempts";
        public const string AckRequestedKey = "ack-requested";
        public const string MessageTypeKey = "message-type";
        public const string AcceptedContentTypesKey = "accepted-content-types";
        public const string DeliverByHeader = "deliver-by";
    }
}
