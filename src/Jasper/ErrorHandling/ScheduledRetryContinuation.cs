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

        public Task Execute(IChannelCallback channel,
            IExecutionContext execution, DateTime utcNow)
        {
            execution.Envelope.ExecutionTime = utcNow.Add(Delay);

            if (channel is IHasNativeScheduling c) return c.MoveToScheduledUntil(execution.Envelope, execution.Envelope.ExecutionTime.Value);

            return execution.Persistence.ScheduleJob(execution.Envelope);
        }

        public override string ToString()
        {
            return $"Schedule Retry in {Delay.TotalSeconds} seconds";
        }

        protected bool Equals(ScheduledRetryContinuation other)
        {
            return Delay.Equals(other.Delay);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScheduledRetryContinuation) obj);
        }

        public override int GetHashCode()
        {
            return Delay.GetHashCode();
        }
    }
}
