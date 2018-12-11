using System;

namespace Jasper.Messaging.Runtime.Invocation
{
    public class ScheduledResponse : ISendMyself
    {
        public ScheduledResponse(object outgoing, TimeSpan delay) : this(outgoing, DateTime.UtcNow.Add(delay))
        {
            Delay = delay;
        }

        public ScheduledResponse(object outgoing, DateTime time)
        {
            Outgoing = outgoing;
            Time = time;
        }

        public object Outgoing { get; }

        public DateTime Time { get; }

        public TimeSpan Delay { get; }

        public Envelope CreateEnvelope(Envelope original)
        {
            var outgoing = original.ForResponse(Outgoing);
            outgoing.ExecutionTime = Time;

            return outgoing;
        }

        public override string ToString()
        {
            return string.Format("Execute {0} at time: {1}", Outgoing, Time);
        }
    }
}
