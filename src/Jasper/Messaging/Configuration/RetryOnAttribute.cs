using System;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using Polly;

namespace Jasper.Messaging.Configuration
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

        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.Retries += _exceptionType.HandledBy().RetryAsync(3);
        }
    }
}
