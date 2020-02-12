using System;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public class MessageSucceededContinuation : IContinuation
    {
        public static readonly MessageSucceededContinuation Instance = new MessageSucceededContinuation();

        private MessageSucceededContinuation()
        {
        }

        public async Task Execute(IMessagingRoot root, IMessageContext context, DateTime utcNow)
        {
            var envelope = context.Envelope;

            try
            {
                await context.SendAllQueuedOutgoingMessages();

                await envelope.Callback.Complete();

                context.Advanced.Logger.MessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                await context.Advanced.SendFailureAcknowledgement("Sending cascading message failed: " + ex.Message);

                context.Advanced.Logger.LogException(ex, envelope.Id, ex.Message);
                context.Advanced.Logger.MessageFailed(envelope, ex);

                await envelope.MoveToErrors(root, ex);
            }
        }
    }
}
