using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Internals.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.Scanning.Conventions
{
    public class FindAllTypesFilter : IRegistrationConvention
    {
        private readonly Type _pluginType;

        public FindAllTypesFilter(Type pluginType)
        {
            _pluginType = pluginType;
        }

        public bool Matches(Type type)
        {
            return type.CanBeCastTo(_pluginType) && type.GetConstructors().Any() && type.CanBeCreated();
        }

        public void ScanTypes(TypeSet types, IServiceCollection services)
        {
            types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Where(Matches).Each(type =>
            {
                services.AddType(determineLeastSpecificButValidType(_pluginType, type), type);
            });
        }

        private static Type determineLeastSpecificButValidType(Type pluginType, Type type)
        {
            if (pluginType.GetTypeInfo().IsGenericTypeDefinition && !type.IsOpenGeneric())
                return type.FindFirstInterfaceThatCloses(pluginType);

            return pluginType;
        }

        public override string ToString()
        {
            return "Find and register all types implementing " + _pluginType.FullName;
        }
    }
}
