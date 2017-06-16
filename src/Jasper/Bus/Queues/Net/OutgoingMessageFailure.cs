using System;

namespace Jasper.Bus.Queues.Net
{
    public class OutgoingMessageFailure
    {
        public Exception Exception { get; set; }
        public OutgoingMessageBatch Batch { get; set; }
    }
}