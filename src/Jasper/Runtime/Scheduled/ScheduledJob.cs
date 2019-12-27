using System;

namespace Jasper.Runtime.Scheduled
{
    public class ScheduledJob
    {
        public ScheduledJob(Guid envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public Guid EnvelopeId { get; }

        public DateTime ReceivedAt { get; set; }
        public DateTimeOffset ExecutionTime { get; set; }

        public string From { get; set; }
        public string MessageType { get; set; }
    }
}
