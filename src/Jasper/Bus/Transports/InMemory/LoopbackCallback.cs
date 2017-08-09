using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.InMemory
{
    public class LoopbackCallback : IMessageCallback
    {
        private readonly LoopbackQueue _queue;
        private readonly LoopbackMessage _message;
        private readonly Uri _destination;

        public bool Successful { get; private set; } = false;
        public bool Failed { get; private set; } = false;

        public LoopbackCallback(LoopbackQueue queue, LoopbackMessage message, Uri destination)
        {
            _queue = queue;
            _message = message;
            _destination = destination;
        }

        public Task MarkSuccessful()
        {
            Successful = true;
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            Failed = true;
            return Task.CompletedTask;
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

        public Task MoveToErrors(ErrorReport report)
        {
            //TODO: what to do with errors?
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            _message.ReplaceId();
            return _queue.Send(_message, envelope.Destination);
        }

        public Task Send(Envelope envelope)
        {

            var message = new LoopbackMessage(envelope, DateTime.UtcNow);

            return _queue.Send(message, envelope.Destination);
        }

        public bool SupportsSend { get; } = true;
    }
}
