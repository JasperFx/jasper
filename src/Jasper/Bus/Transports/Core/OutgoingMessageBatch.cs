using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Core
{
    public class OutgoingMessageBatch
    {
        public OutgoingMessageBatch(Uri destination, IEnumerable<Envelope> messages)
        {
            Destination = destination;
            var messagesList = new List<Envelope>();
            messagesList.AddRange(messages);
            Messages = messagesList;
        }

        public Uri Destination { get; set; }

        public IList<Envelope> Messages { get; }

        public override string ToString()
        {
            return $"Outgoing batch to {Destination} with {Messages.Count} messages";
        }
    }
}
