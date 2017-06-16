using Jasper.Bus.Runtime;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics.Messages
{
    public class MessageSucceeded : ClientMessage
    {
        public MessageSucceeded(Envelope envelope) : base("bus-message-succeeded")
        {
            Envelope = new EnvelopeModel(envelope);
        }

        public EnvelopeModel Envelope { get; }
    }
}
