using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public class OutgoingEnvelope
    {
        public OutgoingEnvelope()
        {
        }

        public OutgoingEnvelope(Envelope envelope)
        {
            Id = envelope.Id;
            OwnerId = envelope.OwnerId;
            Destination = envelope.Destination.ToString();
            DeliverBy = envelope.DeliverBy;

            Body = envelope.Serialize();
        }

        public Guid Id { get; set; }
        public int OwnerId { get; set; }
        public string Destination { get; set; }
        public DateTimeOffset? DeliverBy { get; set; }
        public byte[] Body { get; set; }
    }
}
