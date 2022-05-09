using System;
using Jasper.Configuration;
using Lamar;
using LamarCodeGeneration;

namespace Jasper.Attributes;

/// <summary>
///     Base class to use for applying middleware or other alterations to generic
///     IChains (either RouteChain or HandlerChain)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class ModifyChainAttribute : Attribute
{
    public abstract void Modify(IChain chain, GenerationRules rules, IContainer container);
}
