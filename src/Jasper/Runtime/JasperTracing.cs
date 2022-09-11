using System.Diagnostics;
using Baseline;

namespace Jasper.Runtime;

internal static class JasperTracing
{
    // See https://opentelemetry.io/docs/reference/specification/trace/semantic_conventions/messaging/ for more information

    public const string MessageType = "messaging.message_type";

    // This needs to be the correlation id. Not necessarily the same thing as the message id
    public const string MessagingConversationId = "messaging.conversation_id"; // The Jasper correlation Id
    public const string MessagingMessageId = "messaging.message_id";
    public const string MessagingSystem = "messaging.system"; // Use the destination Uri scheme
    public const string MessagingDestination = "messaging.destination"; // Use the destination Uri

    // TODO -- transport specific tracing
    public const string MessagingDestinationKind = "messaging.destination_kind"; // Not sure this is going to be helpful. queue or topic. Maybe port if TCP basically.
    public const string MessagingTempDestination = "messaging.temp_destination"; // boolean if this is temporary
    public const string PayloadSizeBytes = "messaging.message_payload_size_bytes";


    // Transport specific things
    // messaging.consumer_id
    // messaging.rabbitmq.routing_key



    internal static ActivitySource ActivitySource { get; } = new(
        "Jasper",
        typeof(JasperTracing).Assembly.GetName().Version!.ToString());



    public static Activity? StartExecution(string spanName, Envelope envelope,
        ActivityKind kind = ActivityKind.Internal)
    {
        var activity = envelope.ParentId.IsNotEmpty()
            ? ActivitySource.StartActivity(spanName, kind, envelope.ParentId)
            :ActivitySource.StartActivity(spanName, kind);

        if (activity == null) return null;

        envelope.WriteTags(activity);

        return activity;
    }

    internal static void MaybeSetTag<T>(this Activity activity, string tagName, T? value)
    {
        if (value != null)
        {
            activity.SetTag(tagName, value);
        }
    }
}
