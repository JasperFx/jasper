using System;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    public class RequeueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public RequeueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).Requeue();
        }
    }
}