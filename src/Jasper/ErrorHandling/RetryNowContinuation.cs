using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class RetryNowContinuation : IContinuation
{
    public static readonly RetryNowContinuation Instance = new();

    private readonly TimeSpan? _delay;

    private RetryNowContinuation()
    {
    }

    public RetryNowContinuation(TimeSpan delay)
    {
        _delay = delay;
    }

    public TimeSpan? Delay => _delay;

    public async ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        if (_delay != null)
        {
            await Task.Delay(_delay.Value).ConfigureAwait(false);
        }

        await execution.RetryExecutionNowAsync().ConfigureAwait(false);
    }

    public override string ToString()
    {
        return "Retry Now";
    }
}
