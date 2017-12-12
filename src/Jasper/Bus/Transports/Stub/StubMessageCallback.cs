using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Stub
{
    public class StubMessageCallback : IMessageCallback
    {
        private readonly StubChannel _channel;
        public readonly IList<Envelope> Sent = new List<Envelope>();

        public StubMessageCallback(StubChannel channel)
        {
            _channel = channel;
        }

        public bool MarkedSucessful { get; set; }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public DateTime? DelayedTo { get; set; }

        public bool WasMovedToErrors { get; set; }

        public bool Requeued { get; set; }

        public Task MarkComplete()
        {
            MarkedSucessful = true;
            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            WasMovedToErrors = true;
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            Requeued = true;


            return _channel.EnqueueOutgoing(envelope);
        }

        public Task MoveToDelayedUntil(DateTimeOffset time, Envelope envelope)
        {
            throw new NotImplementedException();
        }
    }
}
