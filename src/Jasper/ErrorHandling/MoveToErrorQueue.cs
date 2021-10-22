using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public async Task Execute(IExecutionContext execution,
            DateTime utcNow)
        {
            await execution.SendFailureAcknowledgement(execution.Envelope,
                $"Moved message {execution.Envelope.Id} to the Error Queue.\n{Exception}");

            await execution.MoveToDeadLetterQueue(Exception);

            execution.Logger.MessageFailed(execution.Envelope, Exception);
            execution.Logger.MovedToErrorQueue(execution.Envelope, Exception);


        }

        public override string ToString()
        {
            return "Move to Error Queue";
        }

        protected bool Equals(MoveToErrorQueue other)
        {
            return Equals(Exception, other.Exception);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MoveToErrorQueue) obj);
        }

        public override int GetHashCode()
        {
            return (Exception != null ? Exception.GetHashCode() : 0);
        }
    }
}
