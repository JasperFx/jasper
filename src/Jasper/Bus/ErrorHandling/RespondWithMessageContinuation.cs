using System;
using System.Threading.Tasks;
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

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.SendOutgoingMessage(envelope, Message);
            return Task.CompletedTask;
        }
    }
}