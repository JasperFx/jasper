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

        public const string SerializedEnvelope = "binary/envelope";
        public const string ScheduledEnvelope = "scheduled-envelope";

        public const string Durable = "durable";
        public static readonly string Local = "local";

        public const string Default = "default";
        public const string Replies = "replies";
        public const string Retries = "retries";

        public static readonly Uri RetryUri = "local://retries".ToUri();
        public static readonly Uri RepliesUri = "local://replies".ToUri();
        public static readonly Uri ScheduledUri = "local://delayed".ToUri();

        public static readonly Uri DurableLocalUri = "local://durable/durable".ToUri();
        public static readonly Uri LocalUri = "local://".ToUri();

        public static readonly int AnyNode = 0;
    }
}
