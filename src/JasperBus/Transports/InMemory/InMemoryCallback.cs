using System;
using System.Linq;
using JasperBus.Runtime;
using System.Threading.Tasks;

namespace JasperBus.Transports.InMemory
{
    public class InMemoryCallback : IMessageCallback
    {
        private readonly InMemoryQueue _queue;
        private readonly InMemoryMessage _message;

        public InMemoryCallback(InMemoryQueue queue, InMemoryMessage message)
        {
            _queue = queue;
            _message = message;
        }

        public void MarkSuccessful()
        {
            //TODO
        }

        public void MarkFailed(Exception ex)
        {
            //TODO
        }

        public void MoveToDelayedUntil(DateTime time)
        {
            //TODO
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
            var uri = envelope.Destination;

            var message = new InMemoryMessage
            {
                Id = Guid.NewGuid(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow,
                Queue = uri.Segments.Last()
            };

           return  _queue.Send(message, envelope.Destination);
        }

        public bool SupportsSend { get; } = true;
    }
}
