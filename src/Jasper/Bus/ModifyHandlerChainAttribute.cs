using System;
using Jasper.Bus.Model;
using Jasper.Configuration;

namespace Jasper.Bus
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class ModifyHandlerChainAttribute : Attribute, IModifyChain<HandlerChain>
    {
        public abstract void Modify(HandlerChain chain);
    }
}