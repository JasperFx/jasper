namespace Jasper.Messaging.Runtime
{
    public partial class Envelope
    {
        private const string SentAttemptsHeaderKey = "sent-attempts";
        private const string OriginalIdKey = "original-id";
        private const string SagaIdKey = "saga-id";
        private const string IdKey = "id";
        private const string ParentIdKey = "parent-id";
        private const string ContentTypeKey = "content-type";
        private const string SourceKey = "source";
        public const string ChannelKey = "channel";
        private const string ReplyRequestedKey = "reply-requested";
        private const string ResponseIdKey = "response";
        private const string DestinationKey = "destination";
        private const string ReplyUriKey = "reply-uri";
        private const string ExecutionTimeKey = "time-to-send";
        private const string ReceivedAtKey = "received-at";
        private const string AttemptsKey = "attempts";
        private const string AckRequestedKey = "ack-requested";
        private const string MessageTypeKey = "message-type";
        private const string AcceptedContentTypesKey = "accepted-content-types";
        private const string DeliverByHeader = "deliver-by";

    }
}
