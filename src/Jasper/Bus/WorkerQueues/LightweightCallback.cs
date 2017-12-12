using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;

namespace Jasper.Bus.WorkerQueues
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly IWorkerQueue _queue;

        public LightweightCallback(IWorkerQueue queue)
        {
            _queue = queue;
        }

        public Task MarkComplete()
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            return _queue.Enqueue(envelope);
        }

        public Task MoveToDelayedUntil(DateTimeOffset time, Envelope envelope)
        {
            _queue.DelayedJobs.Enqueue(time, envelope);
            return Task.CompletedTask;
        }
    }
}
