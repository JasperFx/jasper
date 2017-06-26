using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.New.Lightweight
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly QueueReceiver _queue;

        // TODO -- Use the in memory reply queue?
        public LightweightCallback(QueueReceiver queue)
        {
            _queue = queue;
        }

        public Task MarkSuccessful()
        {
            // Nothing
        }

        public Task MarkFailed(Exception ex)
        {
            // Nothing
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
            // TODO -- move right back in
            throw new NotImplementedException();
        }

        public Task Send(Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public bool SupportsSend { get; } = false;
    }
}