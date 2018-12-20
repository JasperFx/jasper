using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public Task Execute(IMessageContext context, DateTime utcNow)
        {
            return context.MoveToErrorQueue(Exception, utcNow);
        }

        public override string ToString()
        {
            return "Move to Error Queue";
        }
    }
}
