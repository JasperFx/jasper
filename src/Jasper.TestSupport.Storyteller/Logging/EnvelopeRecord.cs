using Jasper.Messaging.Runtime;

namespace Jasper.TestSupport.Storyteller.Logging
{
    public class EnvelopeRecord
    {
        public EnvelopeRecord(Envelope envelope, long time, string message, string serviceName)
        {
            Envelope = envelope;
            Time = time;
            Message = message;
            ServiceName = serviceName;

            AttemptNumber = envelope.Attempts;
        }

        public Envelope Envelope { get; }
        public long Time { get; }
        public string Message { get; }
        public string ServiceName { get; }

        public int AttemptNumber { get;  }

        public string ExceptionText { get; set; }
    }
}
