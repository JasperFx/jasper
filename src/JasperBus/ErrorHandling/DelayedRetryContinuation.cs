using System;
using System.Threading.Tasks;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class DelayedRetryContinuation : IContinuation
    {
        public DelayedRetryContinuation(TimeSpan delay)
        {
            Delay = delay;
        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            envelope.Callback.MoveToDelayedUntil(utcNow.Add(Delay));
            return Task.CompletedTask;
        }

        public TimeSpan Delay { get; }
    }
}