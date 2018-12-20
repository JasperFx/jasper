using System;
using Jasper.Configuration;
using Jasper.Messaging.Model;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    ///     Specify the maximum number of attempts to process a received message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MaximumAttemptsAttribute : ModifyHandlerChainAttribute
    {
        private readonly int _attempts;

        public MaximumAttemptsAttribute(int attempts)
        {
            _attempts = attempts;
        }

        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.Retries.MaximumAttempts = _attempts;
        }
    }
}
