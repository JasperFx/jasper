using System;
using Baseline;
using BlueMilk.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk
{
    public class ConcreteFamilyPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsGenericTypeDefinition) return null;
            if (!type.IsConcrete()) return null;

            if (serviceGraph.CouldBuild(type))
            {
                return new ServiceFamily(type, new ConstructorInstance(type, type, ServiceLifetime.Transient));
            }

            return null;
        }
    }
}