using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly ILoopbackQueue _retries;

        public LightweightCallback(ILoopbackQueue retries)
        {
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
            return _retries.Send(envelope, LoopbackTransport.Retries);
        }

        public Task Send(Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public bool SupportsSend { get; } = false;
        public string TransportScheme { get; } = "jasper";
    }
}
