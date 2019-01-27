using System;
using System.Threading.Tasks;

namespace Jasper.Messaging
{
    public static class MessageContextExtensions
    {
        public static async Task MoveToErrorQueue(this IMessageContext context, Exception exception, DateTime utcNow)
        {
            var envelope = context.Envelope;


            await context.Advanced.SendFailureAcknowledgement(
                $"Moved message {envelope.Id} to the Error Queue.\n{exception}");

            await envelope.Callback.MoveToErrors(envelope, exception);

            context.Advanced.Logger.MessageFailed(envelope, exception);
            context.Advanced.Logger.MovedToErrorQueue(envelope, exception);
        }

        public static void MarkFailure(this IMessageContext context, Exception ex)
        {
            context.Envelope.MarkCompletion(false);
            context.Advanced.Logger.LogException(ex, context.Envelope.Id, "Failure during message processing execution");
            context.Advanced.Logger.ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete
        }
    }
}