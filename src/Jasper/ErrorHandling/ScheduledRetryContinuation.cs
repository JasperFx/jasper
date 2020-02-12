using System;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class ScheduledRetryContinuation : IContinuation
    {
        public ScheduledRetryContinuation(TimeSpan delay)
        {
            Delay = delay;
        }

        public TimeSpan Delay { get; }

        public Task Execute(IMessagingRoot root, IMessageContext context, DateTime utcNow)
        {
            var envelope = context.Envelope;
            return context.MoveToScheduledUntil(root, utcNow.Add(Delay));
        }

        public override string ToString()
        {
            return $"Schedule Retry in {Delay.TotalSeconds} seconds";
        }
    }
}
