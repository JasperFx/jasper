using System;
using System.Threading.Tasks;

namespace JasperBus.Runtime.Invocation
{
    public class ChainFailureContinuation : IContinuation
    {
        public ChainFailureContinuation(Exception exception)
        {
            Exception = exception;
        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.SendFailureAcknowledgement(envelope, "Message handler failed");
            envelope.Callback.MarkFailed(Exception);
            
            if (envelope.Message == null)
            {
                context.Error(envelope.CorrelationId, "Error trying to execute a message of type " + envelope.Headers[Envelope.MessageTypeKey], Exception);
            }
            else
            {
                context.Error(envelope.CorrelationId, envelope.Message.ToString(), Exception);
            }

            return Task.CompletedTask;
        }

        public Exception Exception { get; }
    }
}