using System;
using Baseline;
using BlueMilk.Codegen;
using StructureMap;
using StructureMap.Pipeline;

namespace Jasper.Util.StructureMap
{

    [Obsolete("going away when we pull out StructureMap")]
    public class StructureMapServices : IVariableSource
    {
        public static readonly Variable Root;

        private readonly IContainer _container;

        static StructureMapServices()
        {
            Root = new InjectedField(typeof(IContainer), "root");
        }

        public StructureMapServices(IContainer container)
        {
            _container = container;
        }

        public bool Matches(Type type)
        {
            if (type.IsSimple()) return false;

            if (type == typeof(IContainer)) return true;

            return !type.IsSimple() && (_container.Model.HasDefaultImplementationFor(type) || type.IsConcrete());
        }


        public Variable Create(Type type)
        {
            if (type == typeof(IContainer))
            {
                return buildNestedContainer();
            }

            if (_container.Model.HasDefaultImplementationFor(type))
            {
                if (_container.Model.For(type).Default.Lifecycle is SingletonLifecycle)
                {
                    return new InjectedField(type);
                }

                return new ServiceVariable(type, buildNestedContainer());
            }

            if (type.IsConcreteWithDefaultCtor())
            {
                return new NoArgCreationVariable(type);
            }

            if (type.IsConcrete())
            {
                return new ServiceVariable(type, buildNestedContainer());
            }

            return null;
        }

        private static NestedContainerVariable buildNestedContainer()
        {
            return new NestedContainerVariable(new NestedContainerCreation(Root));
        }
    }
}
