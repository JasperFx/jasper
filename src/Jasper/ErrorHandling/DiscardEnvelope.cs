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

    public async ValueTask ExecuteAsync(IExecutionContext execution,
        IJasperRuntime runtime,
        DateTimeOffset now)
    {
        try
        {
            execution.Logger.DiscardedEnvelope(execution.Envelope!);
            await execution.CompleteAsync();
        }
        catch (Exception? e)
        {
            execution.Logger.LogException(e);
        }
    }

    public string Description => "Discard the message";
    public IContinuation Build(Exception ex, Envelope envelope)
    {
        return this;
    }
}
