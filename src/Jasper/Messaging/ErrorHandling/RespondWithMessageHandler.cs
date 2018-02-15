using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
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

        public override string ToString()
        {
            return $"Response with message if Ex is {typeof(T).Name}";
        }
    }
}
