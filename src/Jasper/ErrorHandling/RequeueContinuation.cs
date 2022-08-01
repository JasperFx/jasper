using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling.New;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class RequeueContinuation : IContinuation, IContinuationSource
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

    public string Description { get; } = "Defer or Re-queue the message for later processing";

    public IContinuation Build(Exception ex, Envelope envelope)
    {
        return this;
    }
}
