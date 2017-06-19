using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.InMemory
{
    public class InMemoryCallback : IMessageCallback
    {
        private readonly InMemoryQueue _queue;
        private readonly InMemoryMessage _message;
        private readonly Uri _destination;

        public bool Successful { get; private set; } = false;
        public bool Failed { get; private set; } = false;

        public InMemoryCallback(InMemoryQueue queue, InMemoryMessage message, Uri destination)
        {
            _queue = queue;
            _message = message;
            _destination = destination;
        }

        public void MarkSuccessful()
        {
            Successful = true;
        }

        public void MarkFailed(Exception ex)
        {
            Failed = true;
        }

        public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
        {
            var now = DateTime.UtcNow;
            time = time.ToUniversalTime();
            if (time > now)
            {
                delayedJobs.Enqueue(time, envelope);
                return Task.CompletedTask;
            }

            return _queue.Send(_message, _destination);
        }

        public void MoveToErrors(ErrorReport report)
        {
            //TODO: what to do with errors?
        }

        public Task Requeue(Envelope envelope)
        {
            _message.Id = Guid.NewGuid();
            return _queue.Send(_message, envelope.Destination);
        }

        public Task Send(Envelope envelope)
        {
            var message = new InMemoryMessage
            {
                Id = Guid.NewGuid(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow
            };

            return _queue.Send(message, envelope.Destination);
        }

        public bool SupportsSend { get; } = true;
    }
}
