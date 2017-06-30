using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues
{
    public class OutgoingMessage : Envelope
    {
        private const string SentAttemptsHeaderKey = "sent-attempts";

        public Uri Destination { get; set; }
        public DateTime? DeliverBy { get; set; }
        public int? MaxAttempts { get; set; }

        public int SentAttempts
        {
            get
            {
                if (Headers.ContainsKey(SentAttemptsHeaderKey))
                {
                    return int.Parse(Headers[SentAttemptsHeaderKey]);
                }
                return 0;
            }
            set { Headers[SentAttemptsHeaderKey] = value.ToString(); }
        }
    }
}
