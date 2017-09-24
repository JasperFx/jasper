using System;
using System.Collections.Generic;
using System.Reflection;
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

        public IBuildPlan FindDefault(Type interfaceType)
        {
            throw new NotImplementedException();
        }

        public bool CanBuildType(Type type)
        {
            // check if has explicit registration, if concrete w/ new(), if IEnumerable<T>, T[], IList<T>, IReadOnlyList<T>,
            // close an open generic

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

    // Really a visitor to go down through things
    public class BuildPlanCompiler
    {
        private readonly IList<IBuildPlan> _visited = new List<IBuildPlan>();
    }

    public interface IBuildPlan
    {
        Type ServiceType { get; }
        ServiceLifetime Lifetime { get; }

        void DetermineDependencies(ServiceGraph services);

        bool CanBeInlined { get; }

        IBuildPlan[] ImmediateDependencies { get; }
    }

    // TODO -- build these completely independently?
    public class ConcreteBuildPlan : IBuildPlan
    {
        private readonly ServiceDescriptor _descriptor;
        public Type ServiceType => _descriptor.ServiceType;
        public Type ImplementationType => _descriptor.ImplementationType;
        public ServiceLifetime Lifetime => _descriptor.Lifetime;

        public ConcreteBuildPlan(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public void DetermineDependencies(ServiceGraph services)
        {
            throw new NotImplementedException();
        }

        public IBuildPlan[] ImmediateDependencies { get; }

        public bool CanBeInlined { get; }
    }
}
