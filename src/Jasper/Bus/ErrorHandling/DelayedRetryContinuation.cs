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
            context.DelayedJobs.Enqueue(utcNow.Add(Delay), envelope);
            envelope.Callback.MarkSuccessful();
            return Task.CompletedTask;
        }

        public TimeSpan Delay { get; }
    }
}
