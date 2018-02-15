using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class RespondWithMessageContinuation : IContinuation
    {
        public RespondWithMessageContinuation(object message)
        {
            Message = message;
        }

        public object Message { get; }

        public Task Execute(IMessageContext context, DateTime utcNow)
        {
            context.Advanced.EnqueueCascading(Message);
            return Task.CompletedTask;
        }
    }
}
