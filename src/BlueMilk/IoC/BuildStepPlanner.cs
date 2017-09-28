using System;
using System.Collections.Generic;
using System.Linq;
using BlueMilk.Codegen;
using BlueMilk.Util;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC
{
    public class BuildStepPlanner
    {
        public Type ConcreteType { get; }
        private readonly ServiceGraph _graph;
        private readonly IMethodVariableSource _method;
        private readonly IList<BuildStep> _visited = new List<BuildStep>();
        private readonly IList<BuildStep> _all = new List<BuildStep>();
        private readonly Stack<BuildStep> _chain = new Stack<BuildStep>();

        public BuildStepPlanner(Type concreteType, ServiceGraph graph, IMethodVariableSource method)
        {
            if (!concreteType.IsConcrete()) throw new ArgumentOutOfRangeException(nameof(concreteType), "Must be a concrete type");

            ConcreteType = concreteType;
            _graph = graph;
            _method = method;


            var ctor = graph.ChooseConstructor(concreteType);
            if (ctor == null)
            {
                CanBeReduced = false;
            }
            else
            {
                Top = new ConstructorBuildStep(concreteType, concreteType, ServiceLifetime.Scoped, ctor);
                Visit(Top);
            }
        }

        public ConstructorBuildStep Top { get; private set; }

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

            var @default = _graph.FindDefault(type);
            if (@default?.ImplementationType != null)
            {
                var ctor = _graph.ChooseConstructor(@default.ImplementationType);
                if (ctor != null)
                {
                    return new ConstructorBuildStep(type, @default.ImplementationType, @default.Lifetime, ctor);
                }
            }

            // split on T[], IList<T>, IEnumerable<T>, IReadOnlyList<T>

            return null;
        }
    }
}
