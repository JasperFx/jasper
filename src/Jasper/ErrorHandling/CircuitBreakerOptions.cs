using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.Dates;
using Jasper.ErrorHandling.Matches;

namespace Jasper.ErrorHandling;

public class CircuitBreakerOptions
{
    public int MinimumThreshold { get; set; } = 10;
    public int FailurePercentageThreshold { get; set; } = 15;
    public TimeSpan PauseTime { get; set; } = 5.Minutes();
    public TimeSpan TrackingPeriod { get; set; } = 10.Minutes();

    public TimeSpan SamplingPeriod { get; set; } = 250.Milliseconds();

    public CircuitBreakerOptions Exclude<T>(Func<T, bool>? filter = null) where T : Exception
    {
        return this;
    }

    public CircuitBreakerOptions Include<T>(Func<T, bool>? filter = null) where T : Exception
    {
        return this;
    }

    internal IExceptionMatch ToExceptionMatch()
    {
        throw new NotImplementedException();
    }

    internal void AssertValid()
    {
        var messages = validate().ToArray();
        if (messages.Any())
        {
            throw new InvalidCircuitBreakerException(messages);
        }
    }

    private IEnumerable<string> validate()
    {
        if (MinimumThreshold < 0) yield return $"{nameof(MinimumThreshold)} must be greater than 0";

        if (FailurePercentageThreshold <= 5) yield return $"{nameof(FailurePercentageThreshold)} must be at least 5";

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (PauseTime == null) yield return $"{nameof(PauseTime)} cannot be null";

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (TrackingPeriod == null) yield return $"{nameof(TrackingPeriod)} cannot be null";
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (SamplingPeriod == null) yield return $"{nameof(SamplingPeriod)} cannot be null";
    }


}
