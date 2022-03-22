using System;

namespace Jasper.Tracking
{
    public class EnvelopeRecord
    {
        public Envelope? Envelope { get; }
        public long SessionTime { get; }
        public Exception? Exception { get; }
        public EventType EventType { get; }

        public EnvelopeRecord(EventType eventType, Envelope? envelope, long sessionTime, Exception? exception)
        {
            Envelope = envelope;
            SessionTime = sessionTime;
            Exception = exception;
            EventType = eventType;
            AttemptNumber = envelope.Attempts;
        }

        public int AttemptNumber { get; }

        public bool IsComplete { get; internal set; }
        public string? ServiceName { get; set; }
        public int UniqueNodeId { get; set; }

        public override string ToString()
        {
            var icon = IsComplete ? "+" : "-";
            return $"{icon} Service: {ServiceName}, Id: {Envelope.Id}, {nameof(SessionTime)}: {SessionTime}, {nameof(EventType)}: {EventType}, MessageType: {Envelope.MessageType} at node #{UniqueNodeId} --> {IsComplete}";
        }


    }
}
