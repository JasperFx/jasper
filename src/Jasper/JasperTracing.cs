using System.Diagnostics;

namespace Jasper;

internal static class JasperTracing
{
    // See https://opentelemetry.io/docs/reference/specification/trace/semantic_conventions/messaging/ for more information

    public const string MessageType = "messaging.message_type";

    // This needs to be the correlation id. Not necessarily the same thing as the message id
    public const string MessagingConversationId = "messaging.conversation_id"; // The Jasper correlation Id
    public const string MessagingMessageId = "messaging.message_id";
    public const string MessagingSystem = "messaging.system"; // Use the destination Uri scheme
    public const string MessagingDestination = "messaging.destination"; // Use the destination Uri
    public const string MessagingDestinationKind = "messaging.destination_kind"; // Not sure this is going to be helpful. queue or topic. Maybe port if TCP basically.
    public const string MessagingTempDestination = "messaging.temp_destination"; // boolean if this is temporary

    // messaging.message_payload_size_bytes -- content_length
    // messaging.consumer_id
    // messaging.rabbitmq.routing_key



    public const string Local = "local";

    internal static ActivitySource ActivitySource { get; } = new(
        "Jasper",
        typeof(JasperTracing).Assembly.GetName().Version!.ToString());

    public static Activity StartExecution(string spanName, Envelope envelope,
        ActivityKind kind = ActivityKind.Internal)
    {
        var activity = ActivitySource.StartActivity(spanName, kind) ?? new Activity(spanName);
        activity.SetTag(MessagingSystem, Local);
        activity.SetTag(MessagingMessageId, envelope.Id);
        activity.SetTag(MessagingConversationId, envelope.CorrelationId);
        activity.SetTag(MessageType, envelope.MessageType); // Jasper specific
        if (envelope.CausationId != null)
        {
            activity.SetParentId(envelope.CausationId);
        }


        return activity;
    }
}
