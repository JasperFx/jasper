using System;
using Jasper.Configuration;
using JasperBus.Model;

namespace JasperBus
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class ModifyHandlerChainAttribute : Attribute, IModifyChain<HandlerChain>
    {
        public abstract void Modify(HandlerChain chain);
    }
}