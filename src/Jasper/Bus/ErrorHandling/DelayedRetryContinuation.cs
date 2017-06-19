using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class DelayedRetryContinuation : IContinuation
    {
        public DelayedRetryContinuation(TimeSpan delay)
        {
            Delay = delay;
        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            envelope.Callback.MoveToDelayedUntil(envelope, context.DelayedJobs, utcNow.Add(Delay));
            return Task.CompletedTask;
        }

        public TimeSpan Delay { get; }
    }
}
