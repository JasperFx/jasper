using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class MoveToErrorQueueHandler<T> : IErrorHandler, IContinuationSource where T : Exception
    {
        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            if (ex is T) return new MoveToErrorQueue(ex);

            return null;
        }

        public override string ToString()
        {
            return $"Move to Error Queue if ex is {typeof(T).Name}";
        }
    }
}
