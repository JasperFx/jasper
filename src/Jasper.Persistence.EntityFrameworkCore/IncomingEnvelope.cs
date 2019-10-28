using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public class IncomingEnvelope
    {
        public IncomingEnvelope()
        {
        }

        public IncomingEnvelope(Envelope envelope)
        {
            Id = envelope.Id;
            OwnerId = envelope.OwnerId;
            Status = envelope.Status;
            ExecutionTime = envelope.ExecutionTime;
            Attempts = envelope.Attempts;


            Body = envelope.Serialize();
        }

        public Guid Id { get; set; }
        public int OwnerId { get; set; }

        public string Status { get; set; }
        public DateTimeOffset? ExecutionTime { get; set; }
        public int Attempts { get; set; }
        public byte[] Body { get; set; }
    }
}
