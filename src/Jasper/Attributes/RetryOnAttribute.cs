using System;
using Jasper.ErrorHandling;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;
using Polly;

namespace Jasper.Attributes
{
    /// <summary>
    ///     Applies an error policy that a message should be retried
    ///     whenever processing encounters the designated exception type
    /// </summary>
    public class RetryOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;
        private readonly int _attempts;

        public RetryOnAttribute(Type exceptionType, int attempts = 3)
        {
            _exceptionType = exceptionType;
            _attempts = attempts;
        }

        public override void Modify(HandlerChain chain, GenerationRules rules)
        {
            chain.Retries += _exceptionType.HandledBy().RetryAsync(3);
        }
    }
}
