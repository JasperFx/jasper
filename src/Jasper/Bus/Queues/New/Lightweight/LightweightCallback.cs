using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Queues.New.Lightweight
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly QueueReceiver _queue;
        private readonly IInMemoryQueue _retries;

        public LightweightCallback(QueueReceiver queue, IInMemoryQueue retries)
        {
            _queue = queue;
            _retries = retries;
        }

        public Task MarkSuccessful()
        {
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
        {
            delayedJobs.Enqueue(time, envelope);
            return Task.CompletedTask;
        }

        public Task MoveToErrors(ErrorReport report)
        {
            // TODO -- something here:)
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            return _retries.Send(envelope, InMemoryTransport.Retries);
        }

        public Task Send(Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public bool SupportsSend { get; } = false;
    }
}
