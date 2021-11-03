using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Jasper
{
    internal static class JasperTracing
    {
        public const string MessageType = "messaging.message_type";
        public const string MessagingConversationId = "messaging.conversation_id";
        public const string MessagingMessageId = "messaging.message_id";
        public const string MessagingSystem = "messaging.system";
        public const string Local = "local";

        internal static ActivitySource ActivitySource { get; } = new(
            "Jasper",
            typeof(JasperTracing).Assembly.GetName().Version.ToString());

        public static Activity StartExecution(string spanName, Envelope envelope,
            ActivityKind kind = ActivityKind.Internal)
        {
            var activity = ActivitySource.StartActivity(spanName, kind);
            activity.SetTag(MessagingSystem, Local);
            activity.SetTag(MessagingMessageId, envelope.Id);
            activity.SetTag(MessagingConversationId, envelope.CorrelationId);
            activity.SetTag(MessageType, envelope.MessageType); // Jasper specific
            activity.SetParentId(envelope.CausationId.ToString());


            return activity;
        }


    }

}
