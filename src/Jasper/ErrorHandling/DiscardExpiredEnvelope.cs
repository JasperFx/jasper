using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class DiscardExpiredEnvelope : IContinuation
{
    public static readonly DiscardExpiredEnvelope Instance = new();

    private DiscardExpiredEnvelope()
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
}
