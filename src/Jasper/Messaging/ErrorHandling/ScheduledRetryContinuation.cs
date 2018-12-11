using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class ScheduledRetryContinuation : IContinuation
    {
        public ScheduledRetryContinuation(TimeSpan delay)
        {
            Delay = delay;
        }

        public TimeSpan Delay { get; }

        public Task Execute(IMessageContext context, DateTime utcNow)
        {
            var envelope = context.Envelope;
            return envelope.Callback.MoveToScheduledUntil(utcNow.Add(Delay), envelope);
        }

        public override string ToString()
        {
            return $"Schedule Retry in {Delay.TotalSeconds} seconds";
        }
    }
}
