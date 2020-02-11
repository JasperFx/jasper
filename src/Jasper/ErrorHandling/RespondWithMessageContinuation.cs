using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling
{
    public class RespondWithMessageContinuation : IContinuation
    {
        public RespondWithMessageContinuation(object message)
        {
            Message = message;
        }

        public object Message { get; }

        public Task Execute(IMessagingRoot root, IMessageContext context, DateTime utcNow)
        {
            context.Advanced.EnqueueCascading(Message);
            return Task.CompletedTask;
        }
    }
}
