using System;
using System.Linq;
using Baseline.Dates;
using Jasper.ErrorHandling;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Attributes;

/// <summary>
///     Applies an error handling policy to schedule a message to be retried
///     in a designated number of seconds after encountering the named exception
/// </summary>
public class RetryLaterAttribute : ModifyHandlerChainAttribute
{
    private readonly Type _exceptionType;
    private readonly int[] _seconds;

    public RetryLaterAttribute(Type exceptionType, params int[] seconds)
    {
        _exceptionType = exceptionType;
        _seconds = seconds;
    }

    public override void Modify(HandlerChain chain, GenerationRules rules)
    {
        var timeSpans = _seconds.Select(x => x.Seconds()).ToArray();
        chain.OnExceptionOfType(_exceptionType)
            .RetryLater(timeSpans);
    }
}
