using System;
using System.Collections.Generic;
using JasperBus.Runtime;

namespace JasperBus.Tracking
{

    public class MessageTrack
    {
        public static string ToKey(Envelope envelope, string activity)
        {
            return $"{envelope.CorrelationId}/{envelope.Destination}/{activity}";
        }

        public string CorrelationId { get; }
        public string Activity { get; }
        public DateTime Recorded { get; } = DateTime.UtcNow;

        public MessageTrack(Envelope envelope, string activity)
        {
            CorrelationId = envelope.CorrelationId;
            Activity = activity;

            Key = ToKey(envelope, activity);
        }

        public string Key { get; }
        public bool Completed { get; private set; }

        public void Finish(Envelope envelope, Exception ex = null)
        {
            Completed = true;
            ExceptionText = ex?.ToString();
            Headers = envelope.Headers;
        }

        public IDictionary<string, string> Headers { get; private set; }

        public string ExceptionText { get; private set; }
    }

}