using System;
using System.Threading.Tasks;

namespace Jasper.Runtime;

internal class NullContinuation : IContinuation
{
    public static readonly NullContinuation Instance = new NullContinuation();

    public ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        return ValueTask.CompletedTask;
    }
}
