using Jasper.Bus.Runtime;

namespace Jasper.Diagnostics.Messages
{
    public class MessageSent : ClientMessage
    {
        public MessageSent(Envelope envelope) : base("bus-message-sent")
        {
            Envelope = new EnvelopeModel(envelope);
        }

        public EnvelopeModel Envelope { get; }
    }
}
