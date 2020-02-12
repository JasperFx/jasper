using System;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues
{
    public class LightweightCallback : IMessageCallback, IHasNativeScheduling
    {
        private readonly IWorkerQueue _queue;
        private readonly Envelope _envelope;

        public LightweightCallback(IWorkerQueue queue, Envelope envelope)
        {
            _queue = queue;
            _envelope = envelope;
        }

        public Task Complete()
        {
            return Task.CompletedTask;
        }

        public Task Defer()
        {
            return _queue.Enqueue(_envelope);
        }

        public Task MoveToScheduledUntil(DateTimeOffset time)
        {
            _envelope.ExecutionTime = time;
            return _queue.ScheduleExecution(_envelope);
        }
    }
}
