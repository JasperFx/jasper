using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Runtime;
using Microsoft.Extensions.Logging;

namespace Jasper.ErrorHandling;

internal class CompositeContinuation : IContinuation
{
    private readonly IContinuation[] _continuations;

    public CompositeContinuation(params IContinuation[] continuations)
    {
        _continuations = continuations;
    }

    public IReadOnlyList<IContinuation> Inner => _continuations;

    public async ValueTask ExecuteAsync(IExecutionContext execution, IJasperRuntime runtime, DateTimeOffset now)
    {
        foreach (var continuation in _continuations)
        {
            try
            {
                await continuation.ExecuteAsync(execution, runtime, now);
            }
            catch (Exception e)
            {
                runtime.Logger.LogError(e, "Failed while attempting to apply continuation {Continuation} on Envelope {Envelope}", continuation, execution.Envelope);
            }
        }
    }
}
