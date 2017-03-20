using System;
using System.Threading.Tasks;

namespace JasperBus.Runtime.Invocation
{
    public class ChainSuccessContinuation : IContinuation
    {
        public static readonly ChainSuccessContinuation Instance = new ChainSuccessContinuation();

        private ChainSuccessContinuation()
        {

        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            try
            {
                context.SendAllQueuedOutgoingMessages();

                envelope.Callback.MarkSuccessful();

                context.Logger.MessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                context.SendFailureAcknowledgement(envelope, "Sending cascading message failed: " + ex.Message);
                context.Error(envelope.CorrelationId, ex.Message, ex);

                envelope.Callback.MoveToErrors(new ErrorReport(envelope, ex));
            }

            return Task.CompletedTask;
        }

    }
}