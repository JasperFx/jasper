using System;
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

        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            envelope.Callback.MoveToDelayedUntil(utcNow.Add(Delay));
        }

        public TimeSpan Delay { get; }
    }
}