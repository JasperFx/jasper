using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
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
