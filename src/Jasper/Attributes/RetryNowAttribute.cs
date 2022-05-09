using System;
using Jasper.ErrorHandling;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Attributes;

/// <summary>
///     Applies an error policy that a message should be retried
///     whenever processing encounters the designated exception type
/// </summary>
public class RetryNowAttribute : ModifyHandlerChainAttribute
{
    private readonly int _attempts;
    private readonly Type _exceptionType;

    public RetryNowAttribute(Type exceptionType, int attempts = 3)
    {
        _exceptionType = exceptionType;
        _attempts = attempts;
    }

    public override void Modify(HandlerChain chain, GenerationRules rules)
    {
        chain.OnExceptionOfType(_exceptionType).RetryNow(_attempts);
    }
}
