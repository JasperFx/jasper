namespace Jasper.Bus.Runtime
{
    public partial class Envelope
    {
        public const string SentAttemptsHeaderKey = "sent-attempts";
        public const string OriginalIdKey = "original-id";
        public const string IdKey = "id";
        public const string ParentIdKey = "parent-id";
        public const string ContentTypeKey = "content-type";
        public const string SourceKey = "source";
        public const string ChannelKey = "channel";
        public const string ReplyRequestedKey = "reply-requested";
        public const string ResponseIdKey = "response";
        public const string DestinationKey = "destination";
        public const string ReplyUriKey = "reply-uri";
        public const string ExecutionTimeKey = "time-to-send";
        public const string ReceivedAtKey = "received-at";
        public const string AttemptsKey = "attempts";
        public const string AckRequestedKey = "ack-requested";
        public const string MessageTypeKey = "message-type";
        public const string AcceptedContentTypesKey = "accepted-content-types";
        public const string MaxAttemptsHeader = "max-delivery-attempts";
        public const string DeliverByHeader = "deliver-by";

    }
}
