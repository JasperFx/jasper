using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class ScheduledRetryContinuation : IContinuation
{
    public ScheduledRetryContinuation(TimeSpan delay)
    {
        _delay = delay;
    }

    private readonly TimeSpan _delay;

    public async ValueTask ExecuteAsync(IExecutionContext execution, DateTimeOffset now)
    {
        var scheduledTime = now.Add(_delay);

        await execution.ReScheduleAsync(scheduledTime);
    }

    public override string ToString()
    {
        return $"Schedule Retry in {_delay.TotalSeconds} seconds";
    }

    protected bool Equals(ScheduledRetryContinuation other)
    {
        return _delay.Equals(other._delay);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ScheduledRetryContinuation)obj);
    }

    public override int GetHashCode()
    {
        return _delay.GetHashCode();
    }
}
