using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.WorkerQueues
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly WorkerQueue _queue;

        public LightweightCallback(WorkerQueue queue)
        {
            _queue = queue;
        }

        public Task MarkSuccessful()
        {
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(ErrorReport report)
        {
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            return _queue.Enqueue(envelope);
        }

    }
}
