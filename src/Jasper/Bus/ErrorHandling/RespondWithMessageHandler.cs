using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class RespondWithMessageHandler<T> : IContinuationSource where T : Exception
    {
        private readonly Func<T, Envelope, object> _messageFunc;

        public RespondWithMessageHandler(Func<Exception, Envelope, object> messageFunc)
        {
            _messageFunc = messageFunc;
        }

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            var exception = ex as T;
            if (exception == null)
                return null;

            var message = _messageFunc(exception, envelope);

            return new RespondWithMessageContinuation(message);
        }
    }
}