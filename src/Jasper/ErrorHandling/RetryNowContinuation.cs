using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class RetryNowContinuation : IContinuation
{
    public static readonly RetryNowContinuation Instance = new();

    private RetryNowContinuation()
    {
    }

    public async ValueTask ExecuteAsync(IExecutionContext execution, DateTimeOffset now)
    {
        await execution.RetryExecutionNowAsync();
    }

    public override string ToString()
    {
        return "Retry Now";
    }
}
