using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling.New;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class PauseListenerContinuation : IContinuation, IContinuationSource
{
    public PauseListenerContinuation(TimeSpan pauseTime)
    {
        PauseTime = pauseTime;
    }

    public TimeSpan PauseTime { get; }

    public ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        var agent = runtime.FindListeningAgent(execution.Envelope!.Listener!.Address);
        if (agent != null) return agent.PauseAsync(PauseTime);

        return ValueTask.CompletedTask;
    }

    public string Description => "Pause all message processing on this listener for " + PauseTime;
    public IContinuation Build(Exception ex, Envelope envelope)
    {
        return this;
    }
}
