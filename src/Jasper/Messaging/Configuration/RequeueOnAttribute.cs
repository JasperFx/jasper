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

        public RequeueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.OnException(_exceptionType).Requeue();
        }
    }
}
