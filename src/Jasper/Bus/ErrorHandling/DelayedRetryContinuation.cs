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
            return envelope.Callback.MoveToDelayedUntil(envelope, context.DelayedJobs, utcNow.Add(Delay));
        }

        public TimeSpan Delay { get; }
    }
}
