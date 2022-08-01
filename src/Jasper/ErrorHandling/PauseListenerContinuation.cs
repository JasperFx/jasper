using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling.New;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class PauseListenerContinuation : IContinuation, IContinuationSource
{
    private readonly TimeSpan _pauseTime;

    public PauseListenerContinuation(TimeSpan pauseTime)
    {
        _pauseTime = pauseTime;
    }

    public ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        var agent = runtime.FindListeningAgent(execution.Envelope!.Listener!.Address);
        if (agent != null) return agent.PauseAsync(_pauseTime);

        return ValueTask.CompletedTask;
    }

    public string Description => "Pause all message processing on this listener for " + _pauseTime;
    public IContinuation Build(Exception ex, Envelope envelope)
    {
        return this;
    }
}
