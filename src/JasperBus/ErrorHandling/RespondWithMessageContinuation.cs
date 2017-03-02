using System;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class RespondWithMessageContinuation : IContinuation
    {
        public RespondWithMessageContinuation(object message)
        {
            Message = message;
        }

        public object Message { get; }

        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.SendOutgoingMessage(envelope, Message);
        }
    }
}