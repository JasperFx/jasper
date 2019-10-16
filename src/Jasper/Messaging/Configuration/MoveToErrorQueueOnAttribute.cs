using System;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using LamarCodeGeneration;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    ///     Move the message to the error queues on encountering the named Exception type
    /// </summary>
    public class MoveToErrorQueueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public MoveToErrorQueueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain, GenerationRules rules)
        {
            chain.Retries += _exceptionType.HandledBy()
                .MoveToErrorQueue();
        }
    }
}
