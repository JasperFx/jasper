using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Runtime;

namespace Jasper.ErrorHandling.New;

internal class FailureRule
{
    private readonly IExceptionMatch _match;
    private readonly Func<Exception,bool> _filter;
    private readonly List<FailureSlot> _slots = new();

    public FailureRule(IExceptionMatch match)
    {
        _match = match;
        _filter = match.ToFilter();
    }

    public bool TryCreateContinuation(Exception ex, Envelope env, out IContinuation continuation)
    {
        if (_filter(ex))
        {
            if (env.Attempts == 0)
            {
                env.Attempts = 1;
            }

            var slot = _slots.FirstOrDefault(x => x.Attempt == env.Attempts);
            continuation = slot?.Build(ex, env) ?? new MoveToErrorQueue(ex);
            return true;
        }

        continuation = NullContinuation.Instance;
        return false;
    }

    public FailureSlot this[int attempt] => _slots[attempt - 1];

    public FailureSlot AddSlot(IContinuationSource source)
    {
        var attempt = _slots.Count + 1;
        var slot = new FailureSlot(attempt, source);
        _slots.Add(slot);

        return slot;
    }
}
