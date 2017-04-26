using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace Jasper.Configuration
{
    internal static class HelperExtensions
    {
        public static bool IsGenericEnumerable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static GenericFamilyExpression LifecycleIs(this GenericFamilyExpression instance, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return instance.LifecycleIs(Lifecycles.Singleton);
                case ServiceLifetime.Scoped:
                    return instance.LifecycleIs(Lifecycles.Container);
                case ServiceLifetime.Transient:
                    return instance.LifecycleIs(Lifecycles.Unique);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }

        public static bool HasFamily<TPlugin>(this PluginGraph graph)
        {
            return graph.HasFamily(typeof(TPlugin));
        }
    }
}