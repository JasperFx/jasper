using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using BlueMilk.Util;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC
{

    public class ServiceGraph
    {
        private readonly Dictionary<Type, ConstructorInfo> _constructors = new Dictionary<Type, ConstructorInfo>();

        public ServiceGraph(IServiceCollection services)
        {
        }

        public ConstructorInfo ChooseConstructor(Type type)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor FindDefault(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor[] FindAll(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }

    public class ConstructorFrame : SyncFrame
    {
        private readonly Variable[] _arguments;
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Variable Variable { get; }

        public ConstructorFrame(Type serviceType, Type implementationType, string variableName, Variable[] arguments)
        {
            _arguments = arguments;
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Variable = new Variable(serviceType, variableName, this);
        }

        public int Number { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var arguments = _arguments.Select(x => x.Usage).Join(", ");
            var implementationTypeName = ImplementationType.FullName.Replace("+", ".");
            writer.Write($"var {Variable.Usage} = new {implementationTypeName}({arguments});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(GeneratedMethod chain)
        {
            return _arguments;
        }
    }


    public abstract class BuildStep
    {
        protected readonly IList<BuildStep> _dependencies = new List<BuildStep>();
        public Type ServiceType { get; }

        public BuildStep(Type serviceType, bool canBeReused, bool shouldDispose)
        {
            ServiceType = serviceType;
            CanBeReused = canBeReused;
            ShouldDispose = shouldDispose;
        }

        public bool ShouldDispose { get; }

        public bool CanBeReused { get; }

        /// <summary>
        /// If you are creating multiple instances of the same concrete type, use
        /// this as a suffix on the variable
        /// </summary>
        public int Number { get; set; }

        public abstract IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner);


        public abstract Variable BuildVariable();
    }

    public class KnownVariableBuildStep : BuildStep
    {
        public Variable Variable { get; }

        public KnownVariableBuildStep(Variable variable) : base(variable.VariableType, true, false)
        {
            Variable = variable;
        }

        public override IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner)
        {
            yield break;
        }

        public override Variable BuildVariable()
        {
            return Variable;
        }
    }

    public class BuildStepPlanner
    {
        private readonly ServiceGraph _graph;
        private readonly GeneratedMethod _method;
        private readonly IList<BuildStep> _visited = new List<BuildStep>();
        private readonly IList<BuildStep> _all = new List<BuildStep>();
        private readonly Stack<BuildStep> _chain = new Stack<BuildStep>();

        // TODO -- have this take in GeneratedMethod too
        public BuildStepPlanner(Type concreteType, ServiceGraph graph, GeneratedMethod method)
        {
            _graph = graph;
            _method = method;
        }

        public bool CanBeReduced { get; private set; } = true;

        public void Visit(BuildStep step)
        {
            if (_chain.Contains(step))
            {
                throw new InvalidOperationException("Bi-directional dependencies detected:" + Environment.NewLine + _chain.Select(x => x.ToString()).Join(Environment.NewLine));
            }

            if (_visited.Contains(step))
            {
                return;
            }

            _chain.Push(step);

            foreach (var dep in step.ReadDependencies(this))
            {
                if (dep == null)
                {
                    CanBeReduced = false;
                    return;
                }


                Visit(dep);
            }

            _chain.Pop();
        }

        public BuildStep FindStep(Type type)
        {
            var candidate = _all.FirstOrDefault(x => x.ServiceType == type && x.CanBeReused);
            if (candidate != null) return candidate;

            var step = findStep(type);

            _all.Add(step);

            return step;
        }

        private BuildStep findStep(Type type)
        {
            var variable = _method.TryFindVariable(type);

            if (variable != null) return new KnownVariableBuildStep(variable);

            // split on T[], IList<T>, IEnumerable<T>, IReadOnlyList<T>

            return null;

            throw new NotImplementedException();
        }
    }




    public interface IBuilder<T>
    {
        T Build();
    }



    public class NoArgConstructorBuilder<T> : IBuilder<T> where T : new()
    {
        public T Build()
        {
            return new T();
        }
    }


}
