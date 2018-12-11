using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Tracking
{
    public class MessageTrack
    {
        public MessageTrack(Envelope envelope, string activity)
        {
            CorrelationId = envelope.Id;
            Activity = activity;
            MessageType = envelope.Message?.GetType();

            Key = ToKey(envelope, activity);
        }

        public Guid CorrelationId { get; }
        public string Activity { get; }
        public DateTime Recorded { get; } = DateTime.UtcNow;
        public Type MessageType { get; }

        public string Key { get; }
        public bool Completed { get; private set; }

        public IDictionary<string, string> Headers { get; private set; }

        public string ExceptionText { get; private set; }

        public static string ToKey(Envelope envelope, string activity)
        {
            return $"{envelope.Id}/{envelope.Destination}/{activity}";
        }

        public void Finish(Envelope envelope, Exception ex = null)
        {
            Completed = true;
            ExceptionText = ex?.ToString();
            Headers = envelope.Headers;
        }
    }
}
