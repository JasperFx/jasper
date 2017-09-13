using System;

namespace Jasper.Bus.Runtime.Invocation
{
    public class DelayedResponse : ISendMyself
    {
        private readonly object _outgoing;

        public DelayedResponse(object outgoing, TimeSpan delay) : this(outgoing, DateTime.UtcNow.Add(delay))
        {
            Delay = delay;
        }

        public DelayedResponse(object outgoing, DateTime time)
        {
            _outgoing = outgoing;
            Time = time;
        }

        public object Outgoing => _outgoing;

        public DateTime Time { get; }

        public TimeSpan Delay { get; }

        public Envelope CreateEnvelope(Envelope original)
        {
            var outgoing = original.ForResponse(_outgoing);
            outgoing.ExecutionTime = Time;

            return outgoing;
        }

        public override string ToString()
        {
            return string.Format("Execute {0} at time: {1}", _outgoing, Time);
        }
    }
}