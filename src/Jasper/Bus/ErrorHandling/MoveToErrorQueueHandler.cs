using System;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class MoveToErrorQueueHandler<T> : IErrorHandler, IContinuationSource where T : Exception
    {
        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            if (ex is T) return new MoveToErrorQueue(ex);

            return null;
        }
    }
}