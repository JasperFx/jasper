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

    public async ValueTask ExecuteAsync(IExecutionContext execution,
        DateTimeOffset now)
    {
        try
        {
            await execution.FlushOutgoingMessagesAsync();

            await execution.CompleteAsync();

            execution.Logger.MessageSucceeded(execution.Envelope!);
        }
        catch (Exception? ex)
        {
            await execution.SendFailureAcknowledgementAsync(execution.Envelope!,
                "Sending cascading message failed: " + ex.Message);

            execution.Logger.LogException(ex, execution.Envelope!.Id, ex.Message);
            execution.Logger.MessageFailed(execution.Envelope, ex);

            await new MoveToErrorQueue(ex).ExecuteAsync(execution, now);
        }
    }
}
