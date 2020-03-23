using System;
using System.Threading.Tasks;
using Jasper.Logging;
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

        public Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages, DateTime utcNow)
        {
            envelope.ExecutionTime = utcNow.Add(Delay);

            if (channel is IHasNativeScheduling c) return c.MoveToScheduledUntil(envelope, envelope.ExecutionTime.Value);

            return root.Persistence.ScheduleJob(envelope);
        }

        public override string ToString()
        {
            return $"Schedule Retry in {Delay.TotalSeconds} seconds";
        }
    }
}
