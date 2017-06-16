using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
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