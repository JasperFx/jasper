using System;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Attributes
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

        public override void Modify(HandlerChain chain, GenerationRules rules)
        {
            chain.Retries.MaximumAttempts = _attempts;
        }
    }
}
