using System;
using JasperBus.Model;

namespace JasperBus
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class ModifyHandlerChainAttribute : Attribute
    {
        public abstract void Modify(HandlerChain chain);
    }
}