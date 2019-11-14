using System;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public static class TransportConstants
    {
        public static readonly int ScheduledJobLockId = "scheduled-jobs".GetDeterministicHashCode();
        public static readonly int IncomingMessageLockId = "recover-incoming-messages".GetDeterministicHashCode();
        public static readonly int OutgoingMessageLockId = "recover-outgoing-messages".GetDeterministicHashCode();
        public static readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetDeterministicHashCode();

        public const string Topic = "topic";
        public const string Queue = "queue";
        public const string Subscription = "subscription";
        public const string Routing = "routingkey";

        public const string SerializedEnvelope = "binary/envelope";
        public const string ScheduledEnvelope = "scheduled-envelope";

        public const string Durable = "durable";
        public static readonly string Local = "local";

        public static readonly string Default = "default";
        public static readonly string Replies = "replies";
        public static readonly string Retries = "retries";

        public static readonly Uri RetryUri = "local://retries".ToUri();
        public static readonly Uri RepliesUri = "local://replies".ToUri();
        public static readonly Uri ScheduledUri = "local://delayed".ToUri();

        public static readonly Uri DurableLoopbackUri = "local://durable".ToUri();
        public static readonly Uri LoopbackUri = "local://".ToUri();

        public static readonly string Scheduled = "Scheduled";
        public static readonly string Incoming = "Incoming";
        public static readonly string Outgoing = "Outgoing";

        public static readonly int AnyNode = 0;
    }
}
