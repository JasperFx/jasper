using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Internals.Codegen;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.IoC
{
    public class ConstructorBuildStep : BuildStep
    {
        private readonly ConstructorInfo _ctor;
        private BuildStep[] _dependencies;
        public Type ImplementationType { get; }

        public ConstructorBuildStep(Type serviceType, Type implementationType, ServiceLifetime lifetime, ConstructorInfo ctor) : base(serviceType, lifetime == ServiceLifetime.Scoped, true)
        {
            _ctor = ctor;
            ImplementationType = implementationType;
        }

        public override IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner)
        {
            _dependencies = _ctor.GetParameters().Select(x => planner.FindStep(x.ParameterType)).ToArray();
            return _dependencies;
        }

        protected override Variable buildVariable()
        {
            var argName = Variable.DefaultArgName(ServiceType);
            if (Number > 0)
            {
                argName += Number;
            }

            var args = _dependencies.Select(x => x.Variable).ToArray();

            return new ConstructorFrame(ServiceType, ImplementationType, argName, args).Variable;
        }
    }
}
