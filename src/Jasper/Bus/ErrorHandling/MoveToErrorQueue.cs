using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.SendFailureAcknowledgement(envelope, $"Moved message {envelope.CorrelationId} to the Error Queue.\n{Exception}");

            context.Logger.MessageFailed(envelope, Exception);
            context.Logger.LogException(Exception, envelope.CorrelationId);

            var report = new ErrorReport(envelope, Exception);
            return envelope.Callback.MoveToErrors(report);
        }
    }
}
