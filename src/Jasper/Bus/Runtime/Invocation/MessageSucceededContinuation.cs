using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Invocation
{
    public class MessageSucceededContinuation : IContinuation
    {
        public static readonly MessageSucceededContinuation Instance = new MessageSucceededContinuation();

        private MessageSucceededContinuation()
        {

        }

        public async Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            try
            {
                await context.SendAllQueuedOutgoingMessages();

                await envelope.Callback.MarkComplete();

                context.Logger.MessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                await context.SendFailureAcknowledgement(envelope, "Sending cascading message failed: " + ex.Message);

                context.Logger.LogException(ex, envelope.Id, ex.Message);
                context.Logger.MessageFailed(envelope, ex);

                await envelope.Callback.MoveToErrors(envelope, ex);
            }
        }

    }
}
