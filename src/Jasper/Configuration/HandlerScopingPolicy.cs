using System;
using System.Linq;
using Jasper.Runtime.Handlers;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Configuration;

internal class HandlerScopingPolicy : IFamilyPolicy
{
    private readonly HandlerGraph _handlers;

    public HandlerScopingPolicy(HandlerGraph handlers)
    {
        _handlers = handlers;
    }

    public ServiceFamily? Build(Type type, ServiceGraph serviceGraph)
    {
        if (type.IsConcrete() && matches(type))
        {
            var instance = new ConstructorInstance(type, type, ServiceLifetime.Scoped);
            return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
        }

        return null;
    }

    private bool matches(Type type)
    {
        var handlerTypes = _handlers.Chains.SelectMany(x => x.Handlers)
            .Select(x => x.HandlerType);

        return handlerTypes.Contains(type);
    }
}
