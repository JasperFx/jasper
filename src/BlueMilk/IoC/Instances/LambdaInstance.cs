using System;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Instances
{
    public class LambdaInstance : Instance
    {
        public Func<IServiceProvider, object> Factory { get; }

        public LambdaInstance(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) :
            base(serviceType, serviceType, lifetime)
        {
            Factory = factory;
            Name = serviceType.NameInCode();
        }

        private LambdaInstance(Type serviceType, Type concreteType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, concreteType, lifetime)
        {
            Factory = factory;
            Name = concreteType.NameInCode();
        }

        public static LambdaInstance For<T>(Func<IServiceProvider, T> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return new LambdaInstance(typeof(T), s => factory(s), lifetime);
        }
        
        public static LambdaInstance For<T, TConcrete>(Func<IServiceProvider, TConcrete> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return new LambdaInstance(typeof(T), typeof(TConcrete), s => factory(s), lifetime);
        }

        public override bool RequiresServiceProvider { get; } = true;

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetInstanceFrame(this).Variable;
        }

        protected override IResolver buildResolver(Scope rootScope)
        {
            switch (Lifetime)
            {
                case ServiceLifetime.Transient:
                    return typeof(TransientLambdaResolver<>).CloseAndBuildAs<IResolver>(Factory, ServiceType);

                case ServiceLifetime.Scoped:
                    return typeof(ScopedLambdaResolver<>).CloseAndBuildAs<IResolver>(Factory, ServiceType);

                case ServiceLifetime.Singleton:
                    return typeof(SingletonLambdaResolver<>).CloseAndBuildAs<IResolver>(Factory, rootScope, ServiceType);
            }
            
            throw new ArgumentOutOfRangeException(nameof(Lifetime));
        }

        public override string ToString()
        {
            return $"Lambda Factory of {ServiceType.NameInCode()}";
        }
    }
}