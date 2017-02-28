using System;
using Baseline;
using StructureMap;
using StructureMap.Pipeline;

namespace Jasper.Codegen.StructureMap
{

    public class StructureMapServices : IVariableSource
    {
        public static readonly NestedContainerVariable Nested;
        public static readonly Variable Root;

        private readonly IContainer _container;

        static StructureMapServices()
        {
            Root = new InjectedField(typeof(IContainer), "root");
            Nested = new NestedContainerVariable(Root);
        }

        public StructureMapServices(IContainer container)
        {
            _container = container;
        }

        public bool Matches(Type type)
        {
            if (type.IsSimple()) return false;

            if (type == typeof(IContainer)) return true;

            return !type.IsSimple() && _container.Model.HasDefaultImplementationFor(type);
        }

        public Variable Create(Type type)
        {
            if (type == typeof(IContainer))
            {
                return Nested;
            }

            if (_container.Model.HasDefaultImplementationFor(type))
            {
                if (_container.Model.For(type).Default.Lifecycle is SingletonLifecycle)
                {
                    return new InjectedField(type);
                }

                return new ServiceVariable(type, Nested);
            }

            return null;
        }
    }
}