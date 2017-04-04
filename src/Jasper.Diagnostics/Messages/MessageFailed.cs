using System;
using Jasper.Remotes.Messaging;
using JasperBus.Runtime;

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
