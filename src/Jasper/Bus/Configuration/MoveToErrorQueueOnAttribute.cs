using System;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    /// <summary>
    /// Move the message to the error queues on encountering the named Exception type
    /// </summary>
    public class MoveToErrorQueueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public MoveToErrorQueueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).MoveToErrorQueue();
        }
    }
}
