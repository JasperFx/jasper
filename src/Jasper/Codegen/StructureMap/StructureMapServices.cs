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
        public static readonly Variable Root = new InjectedField(typeof(IContainer), "root");

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

        public ServiceVariable(Type argType, NestedContainerVariable parent) : base(argType, VariableCreation.BuiltByFrame)
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

        protected override Frame toInstantiationFrame()
        {
            return new ResolveFromNestedContainer(this);
        }
    }

    public class ResolveFromNestedContainer : Frame
    {
        private readonly Variable _variable;

        public ResolveFromNestedContainer(Variable variable) : base(false)
        {
            _variable = variable;
        }

        public override void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"var {_variable.Name} = {StructureMapServices.Nested.Name}.GetInstance<{_variable.VariableType.FullName}>();");
            Next?.GenerateCode(generation, writer);
        }
    }



    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable() : base(typeof(IContainer), "nested", VariableCreation.BuiltByFrame)
        {
        }

        public override IEnumerable<Variable> Dependencies
        {
            get { yield return StructureMapServices.Root; }
        }

        protected override Frame toInstantiationFrame()
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