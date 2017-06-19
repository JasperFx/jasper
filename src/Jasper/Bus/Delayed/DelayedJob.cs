using System;
using System.Collections.Concurrent;

namespace Jasper.Bus.Delayed
{
    public class DelayedJob
    {
        public DelayedJob(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public string EnvelopeId { get; }

        public DateTime ReceivedAt { get; set; }
        public DateTime ExecutionTime { get; set; }

        public string From { get; set; }
        public string MessageType { get; set; }
    }
}
