using System;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    ///     Applies an error policy that a message should be retried
    ///     whenever processing encounters the designated exception type
    /// </summary>
    public class RetryOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public RetryOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.OnException(_exceptionType).Retry();
        }
    }
}
