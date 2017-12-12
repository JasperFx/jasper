using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class ScheduledRetryContinuation : IContinuation
    {
        public ScheduledRetryContinuation(TimeSpan delay)
        {
            Delay = delay;
        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            return envelope.Callback.MoveToScheduledUntil(utcNow.Add(Delay), envelope);
        }

        public TimeSpan Delay { get; }
    }
}

