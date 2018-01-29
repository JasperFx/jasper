using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;


namespace BlueMilk.IoC.Instances
{
    public abstract class Instance
    {
        internal IEnumerable<Assembly> ReferencedAssemblies()
        {
            yield return ServiceType.Assembly;
            yield return ImplementationType.Assembly;

            if (ServiceType.IsGenericType)
            {
                foreach (var type in ServiceType.GetGenericArguments())
                {
                    yield return type.Assembly;
                }
            }

            if (ImplementationType.IsGenericType)
            {
                foreach (var type in ImplementationType.GetGenericArguments())
                {
                    yield return type.Assembly;
                }
            }
        }

        
        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        public static Instance For(ServiceDescriptor service)
        {
            if (service.ImplementationInstance is Instance instance) return instance;
            
            if (service.ImplementationInstance != null) return new ObjectInstance(service.ServiceType, service.ImplementationInstance);
            
            if (service.ImplementationFactory != null) return new LambdaInstance(service.ServiceType, service.ImplementationFactory, service.Lifetime);

            return new ConstructorInstance(service.ServiceType, service.ImplementationType, service.Lifetime);
        }
        
        public static bool CanBeCastTo(Type implementationType, Type serviceType)
        {
            if (implementationType == null) return false;

            if (implementationType == serviceType) return true;


            if (serviceType.IsOpenGeneric())
            {
                return GenericsPluginGraph.CanBeCast(serviceType, implementationType);
            }

            if (implementationType.IsOpenGeneric())
            {
                return false;
            }


            return serviceType.GetTypeInfo().IsAssignableFrom(implementationType.GetTypeInfo());
        }

        protected Instance(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (!CanBeCastTo(implementationType, serviceType))
            {
                throw new ArgumentOutOfRangeException(nameof(implementationType), $"{implementationType.FullNameInCode()} cannot be cast to {serviceType.FullNameInCode()}");
            }
            
            ServiceType = serviceType;
            Lifetime = lifetime;
            ImplementationType = implementationType;
        }

        public virtual bool RequiresServiceProvider => Dependencies.Any(x => x.RequiresServiceProvider);

        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
        public string Name { get; set; } = "default";
        
        public bool HasPlanned { get; protected internal set; }

        public void CreatePlan(ServiceGraph services)
        {
            if (HasPlanned) return;

            try
            {
                services.StartingToPlan(this);
                
            }
            catch (Exception e)
            {
                ErrorMessages.Add(e.Message);
                
                services.FinishedPlanning();
                HasPlanned = true;
                return;
            }
            
            // Can't do the planning on open generic types 'cause bad stuff happens
            if (!ServiceType.IsOpenGeneric())
            {
                var dependencies = createPlan(services) ?? Enumerable.Empty<Instance>();

                Dependencies = dependencies.Concat(dependencies.SelectMany(x => x.Dependencies)).Distinct().ToArray();
            }

            services.ClearPlanning();
            HasPlanned = true;
        }


        public abstract Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot);

        protected virtual IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            return Enumerable.Empty<Instance>();
        }

        public readonly IList<string> ErrorMessages = new List<string>();

        
        public Instance[] Dependencies { get; protected set; } = new Instance[0];


        public void Initialize(Scope rootScope)
        {
            if (Resolver != null) throw new InvalidOperationException("The Resolver has already been built for this Instance");
            
            Resolver = buildResolver(rootScope);

            if (Resolver == null)
            {
                Resolver = new ErrorMessageResolver(this);
            }

            Resolver.Hash = GetHashCode();
            Resolver.Name = Name;
        }

        public IResolver Resolver { get; protected set; }

        protected abstract IResolver buildResolver(Scope rootScope);
        

        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Tries to describe how this instance would be resolved at runtime
        /// </summary>
        public virtual string BuildPlan => ToString();


        public sealed override int GetHashCode()
        {
            unchecked
            {
                return HashCode(ServiceType, Name);
            }
        }

        public static int HashCode(Type serviceType, string name = null)
        {
            return (serviceType.GetHashCode() * 397) ^ (name ?? "default").GetHashCode();
        }

        public virtual Instance CloseType(Type serviceType, Type[] templateTypes)
        {
            return null;
        }
    }
}