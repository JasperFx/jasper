using Jasper.Messaging.Runtime;

namespace Jasper.Storyteller.Logging
{
    public class EnvelopeRecord
    {
        public EnvelopeRecord(Envelope envelope, long time, string message, string serviceName)
        {
            Envelope = envelope;
            Time = time;
            Message = message;
            ServiceName = serviceName;
        }

        public Envelope Envelope { get; }
        public long Time { get; }
        public string Message { get; }
        public string ServiceName { get; }

        public string ExceptionText { get; set; }
    }
}
