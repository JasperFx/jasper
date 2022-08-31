using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;

namespace Jasper.Runtime;

public class MessageSucceededContinuation : IContinuation
{
    public static readonly MessageSucceededContinuation Instance = new();

    private MessageSucceededContinuation()
    {
    }

    public async ValueTask ExecuteAsync(IMessageContext context,
        IJasperRuntime runtime,
        DateTimeOffset now)
    {
        try
        {
            await context.FlushOutgoingMessagesAsync();

            await context.CompleteAsync();

            context.Logger.MessageSucceeded(context.Envelope!);
        }
        catch (Exception ex)
        {
            await context.SendFailureAcknowledgementAsync("Sending cascading message failed: " + ex.Message);

            context.Logger.LogException(ex, context.Envelope!.Id, ex.Message);
            context.Logger.MessageFailed(context.Envelope, ex);

            await new MoveToErrorQueue(ex).ExecuteAsync(context, runtime, now);
        }
    }
}
