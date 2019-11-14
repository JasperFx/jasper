using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.WorkerQueues
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

        public Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            envelope.ExecutionTime = time;
            return _queue.ScheduleExecution(envelope);
        }
    }
}
