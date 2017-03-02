using System;
using Baseline;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.SendFailureAcknowledgement(envelope, "Moved message {0} to the Error Queue.\n{1}".ToFormat(envelope.CorrelationId, Exception));

            var report = new ErrorReport(envelope, Exception);
            envelope.Callback.MoveToErrors(report);
        }
    }
}