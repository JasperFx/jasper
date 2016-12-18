using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Codegen.Compilation;
using StructureMap;
using StructureMap.Pipeline;

namespace Jasper.Codegen.StructureMap
{

    public class StructureMapServices : IVariableSource
    {
        public static readonly NestedContainerVariable Nested = new NestedContainerVariable();

        private readonly IContainer _container;

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

    public class ServiceVariable : Variable
    {
        private readonly NestedContainerVariable _parent;

        public ServiceVariable(Type argType, NestedContainerVariable parent) : base(argType)
        {
            _parent = parent;
        }

        public override IEnumerable<Variable> Dependencies
        {
            get
            {
                yield return _parent;
            }
        }
    }

    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable() : base(typeof(IContainer), "nested", VariableCreation.BuiltByFrame)
        {
        }

        public override IEnumerable<Variable> Dependencies
        {
            get
            {
                yield return new InjectedField(typeof(IContainer), "root");
            }
        }

        public override Frame CreateInstantiationFrame()
        {
            return new NestedContainerCreation();
        }
    }

    public class NestedContainerCreation : Frame
    {
        public NestedContainerCreation() : base(false)
        {
        }

        public override void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            writer.UsingBlock("var nested = root.GetNestedContainer()", w =>
            {
                Next?.GenerateCode(generation, writer);
            });
        }
    }


}