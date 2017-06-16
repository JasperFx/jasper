using System;

namespace JasperBus.Queues.Net
{
    public class OutgoingMessageFailure
    {
        public Exception Exception { get; set; }
        public OutgoingMessageBatch Batch { get; set; }
    }
}