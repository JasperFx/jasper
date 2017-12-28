using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlueMilk.Codegen;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC
{
    public class ConstructorBuildStep : BuildStep, IServiceDescriptorBuildStep
    {
        private readonly ConstructorInfo _ctor;
        private BuildStep[] _dependencies;
        public Type ImplementationType { get; }

        public ConstructorBuildStep(ServiceDescriptor descriptor, ConstructorInfo ctor) : this(descriptor.ServiceType,
            descriptor.ImplementationType, descriptor.Lifetime, ctor)
        {
            ServiceDescriptor = descriptor;
        }




        public ConstructorBuildStep(Type serviceType, Type implementationType, ServiceLifetime lifetime, ConstructorInfo ctor) : base(serviceType, lifetime != ServiceLifetime.Transient, true)
        {
            _ctor = ctor;
            ImplementationType = implementationType;
        }

        public ServiceDescriptor ServiceDescriptor { get; }

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

            var variable = new ConstructorFrame(ServiceType, ImplementationType, argName, args).Variable;
            variable.CanBeReused = CanBeReused;

            return variable;
        }
    }
}
