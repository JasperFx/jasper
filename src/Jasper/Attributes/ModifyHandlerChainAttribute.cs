using System;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Attributes;

/// <summary>
///     Base class for attributes that configure how a message is handled by applying
///     middleware or error handling rules
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class ModifyHandlerChainAttribute : Attribute, IModifyChain<HandlerChain>
{
    /// <summary>
    ///     Called by Jasper during bootstrapping before message handlers are generated and compiled
    /// </summary>
    /// <param name="chain"></param>
    /// <param name="rules"></param>
    public abstract void Modify(HandlerChain chain, GenerationRules rules);
}
