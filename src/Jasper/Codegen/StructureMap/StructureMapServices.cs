using System;
using System.Collections.Generic;
using Baseline;
using StructureMap;
using StructureMap.Pipeline;

namespace Jasper.Codegen.StructureMap
{
    public class StructureMapServices : IVariableSource, IVariable
    {
        private readonly IContainer _container;

        public StructureMapServices(IContainer container)
        {
            _container = container;
        }

        public bool Matches(Type type)
        {
            return !type.IsSimple();
        }

        public IVariable Create(Type type)
        {
            if (type == typeof(IContainer))
            {
                return new InjectedField(type);
            }

            if (_container.Model.HasDefaultImplementationFor(type))
            {
                if (_container.Model.For(type).Default.Lifecycle is SingletonLifecycle)
                {
                    return new InjectedField(type);
                }
            }





            throw new NotImplementedException();
        }

        public string Name { get; } = "parent";
        public Type VariableType { get; } = typeof(IContainer);
    }

    public class NestedContainerVariable : IDependentVariable
    {
        private readonly IVariable _parent;

        public NestedContainerVariable(IVariable parent)
        {
            _parent = parent;
        }

        public string Name { get; } = "nested";
        public Type VariableType { get; } = typeof(IContainer);

        public IEnumerable<IVariable> Dependencies
        {
            get
            {
                yield return _parent;
            }
        }
    }


}