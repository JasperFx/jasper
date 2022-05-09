using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class RequeueContinuation : IContinuation
{
    public static readonly RequeueContinuation Instance = new();

    private RequeueContinuation()
    {
    }

    public ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        return execution.DeferAsync();
    }

    public override string ToString()
    {
        return "Defer the message for later processing";
    }
}
