using System;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    ///     Applies an error handling polity to requeue a message if it
    ///     encounters an exception of the designated type
    /// </summary>
    public class RequeueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;
        private readonly int _attempts;

        public RequeueOnAttribute(Type exceptionType, int attempts = 3)
        {
            _exceptionType = exceptionType;
            _attempts = attempts;
        }

        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.Retries += _exceptionType.HandledBy().Requeue(_attempts);
        }
    }
}
