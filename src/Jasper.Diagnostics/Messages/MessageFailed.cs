using System;
using Jasper.Bus.Runtime;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics.Messages
{
    public class MessageFailed : ClientMessage
    {
        public MessageFailed(Envelope envelope, Exception exception) : base("bus-message-failed")
        {
            Envelope = new EnvelopeModel(envelope, exception);
        }

        public EnvelopeModel Envelope { get; }
    }
}
