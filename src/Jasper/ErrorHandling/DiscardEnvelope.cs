using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class DiscardEnvelope : IContinuation, IContinuationSource
{
    public static readonly DiscardEnvelope Instance = new();

    private DiscardEnvelope()
    {
    }

    public async ValueTask ExecuteAsync(IMessageContext context,
        IJasperRuntime runtime,
        DateTimeOffset now)
    {
        try
        {
            context.Logger.DiscardedEnvelope(context.Envelope!);
            await context.CompleteAsync();
        }
        catch (Exception? e)
        {
            context.Logger.LogException(e);
        }
    }

    public string Description => "Discard the message";
    public IContinuation Build(Exception ex, Envelope envelope)
    {
        return this;
    }
}
