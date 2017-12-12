using System;
using System.Collections.Concurrent;

namespace Jasper.Bus.Delayed
{
    public class DelayedJob
    {
        public DelayedJob(Guid envelopeId)
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
