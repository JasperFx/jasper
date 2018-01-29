using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using BlueMilk.IoC;
using BlueMilk.IoC.Enumerables;
using BlueMilk.IoC.Instances;
using BlueMilk.IoC.Lazy;
using BlueMilk.IoC.Resolvers;
using BlueMilk.Scanning.Conventions;
using BlueMilk.Util;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk
{
    public class ServiceGraph : IDisposable, IModel
    {
        private readonly Scope _rootScope;
        private readonly object _familyLock = new object();
        
        
        private readonly Dictionary<Type, ServiceFamily> _families = new Dictionary<Type, ServiceFamily>();
        public readonly IDictionary<Type, IResolver> ByType = new ConcurrentDictionary<Type, IResolver>();
        
        
        public ServiceGraph(IServiceCollection services, Scope rootScope)
        {
            Services = services;



            // This should blow up pretty fast if it's no good
            applyScanners(services).Wait(2.Seconds());
            
            _rootScope = rootScope;
            

            FamilyPolicies = services
                .Where(x => x.ServiceType == typeof(IFamilyPolicy))
                .Select(x => x.ImplementationInstance.As<IFamilyPolicy>())
                .Concat(new IFamilyPolicy[]
                {
                    new EnumerablePolicy(), 
                    new FuncOrLazyPolicy(), 
                    new CloseGenericFamilyPolicy(), 
                    new ConcreteFamilyPolicy(), 
                    new EmptyFamilyPolicy()
                })
                .ToArray();
            
            
            services.RemoveAll(x => x.ServiceType == typeof(IFamilyPolicy));
            
            addScopeResolver<IServiceProvider>(services);
            addScopeResolver<IContainer>(services);
            addScopeResolver<IServiceScopeFactory>(services);
            ByType[typeof(Scope)] = new ScopeResolver();
            
        }

        private async Task applyScanners(IServiceCollection services)
        {
            _scanners = services.Select(x => x.ImplementationInstance).OfType<AssemblyScanner>().ToArray();
            services.RemoveAll(x => x.ServiceType == typeof(AssemblyScanner));

            foreach (var scanner in _scanners)
            {
                await scanner.ApplyRegistrations(services);
            }
                        
        }

        public IFamilyPolicy[] FamilyPolicies { get; }

        private void addScopeResolver<T>(IServiceCollection services)
        {
            var instance = new ScopeInstance<T>();
            services.Add(instance);
        }

        public void Initialize()
        {
            organizeIntoFamilies(Services);


            buildOutMissingResolvers();
        }
        
        
        
        public void RegisterResolver(Instance instance, IResolver resolver)
        {
            if (instance.IsDefault)
            {
                ByType[instance.ServiceType] = resolver;
            }
        }
        
        public void RegisterResolver(Scope rootScope, IEnumerable<Instance> instances)
        {
            // Yes, you really have to filter twice, because the TopologicalSort will throw back
            // in dependencies that might already have a resolver
            foreach (var instance in instances.Where(x => x.Resolver == null).TopologicalSort(x => x.Dependencies, false).Where(x => x.Resolver == null))
            {
                instance.Initialize(rootScope);
                
                RegisterResolver(instance, instance.Resolver);
            }
        }
        
        private readonly object _locker = new object();

        
        public IResolver FindResolver(Type serviceType)
        {
            if (ByType.TryGetValue(serviceType, out var resolver))
            {
                return resolver;
            }

            lock (_locker)
            {
                if (_families.ContainsKey(serviceType)) return _families[serviceType].Default?.Resolver;

                var family = TryToCreateMissingFamily(serviceType);

                return family.Default?.Resolver;
            }

        }
        
        public IResolver FindResolver(Type serviceType, string name)
        {
            if (_families.TryGetValue(serviceType, out var family))
            {
                return family.ResolverFor(name);
            }

            lock (_locker)
            {
                if (_families.ContainsKey(serviceType)) return _families[serviceType].ResolverFor(name);
                
                family = TryToCreateMissingFamily(serviceType);
                
                return family.ResolverFor(name);
            }
        }
        

        private void buildOutMissingResolvers()
        {
            if (_inPlanning) return;

            _inPlanning = true;

            try
            {
                planResolutionStrategies();

                var requiresGenerated = generateDynamicAssembly();

                var noGeneration = instancesWithoutResolver().Where(x => !requiresGenerated.Contains(x));

                RegisterResolver(_rootScope, noGeneration);
                RegisterResolver(_rootScope, requiresGenerated);
            }
            finally
            {
                _inPlanning = false;
            }
        }

        private IEnumerable<Instance> instancesWithoutResolver()
        {
            return AllInstances().Where(x => x.Resolver == null && !x.ServiceType.IsOpenGeneric());
        }

        private Instance[] generateDynamicAssembly()
        {
            var generatedResolvers = instancesWithoutResolver()
                .OfType<GeneratedInstance>()
                .ToArray();


            // TODO -- will need to get at the GenerationRules from somewhere
            var generatedAssembly = new GeneratedAssembly(new GenerationRules("Jasper.Generated"));
            AllInstances().SelectMany(x => x.ReferencedAssemblies())
                .Distinct()
                .Each(a => generatedAssembly.Generation.Assemblies.Fill(a));
            
            
            
            

            foreach (var instance in generatedResolvers)
            {
                instance.GenerateResolver(generatedAssembly);
            }

            
            generatedAssembly.CompileAll();

            return generatedResolvers.OfType<Instance>().ToArray();
        }

        private bool _inPlanning = false;

        private void planResolutionStrategies()
        {
            while (AllInstances().Where(x => !x.ServiceType.IsOpenGeneric()).Any(x => !x.HasPlanned))
            {
                foreach (var instance in AllInstances().Where(x => !x.HasPlanned).ToArray())
                {
                    instance.CreatePlan(this);
                }
            }
        }

        private void organizeIntoFamilies(IServiceCollection services)
        {
            services
                .Where(x => !x.ServiceType.HasAttribute<BlueMilkIgnoreAttribute>())
                
                .GroupBy(x => x.ServiceType)
                .Select(group => buildFamilyForInstanceGroup(services, @group))
                .Each(family => _families.Add(family.ServiceType, family));


        }

        private ServiceFamily buildFamilyForInstanceGroup(IServiceCollection services, IGrouping<Type, ServiceDescriptor> @group)
        {
            if (@group.Key.IsGenericType && !@group.Key.IsOpenGeneric())
            {
                return buildClosedGenericType(@group.Key, services);
            }

            var instances = @group.Select(Instance.For).ToArray();
            return new ServiceFamily(@group.Key, instances);
        }

        private ServiceFamily buildClosedGenericType(Type serviceType, IServiceCollection services)
        {
            var closed = services.Where(x => x.ServiceType == serviceType).Select(Instance.For);

            var templated = services
                .Where(x => x.ServiceType.IsOpenGeneric() && serviceType.Closes(x.ServiceType))
                .Select(Instance.For)
                .Select(instance =>
                {
                    var arguments = serviceType.GetGenericArguments();

                    try
                    {
                        return instance.CloseType(serviceType, arguments);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(x => x != null);
            
            

            var instances = templated.Concat(closed).ToArray();
            
            return new ServiceFamily(serviceType, instances);
        }

        public IServiceCollection Services { get; }

        public IEnumerable<Instance> AllInstances()
        {
            return _families.Values.SelectMany(x => x.All);
        }

        public IReadOnlyDictionary<Type, ServiceFamily> Families => _families;

        public bool HasFamily(Type serviceType)
        {
            return _families.ContainsKey(serviceType);
        }
        
        public ServiceFamily ResolveFamily(Type serviceType)
        {
            if (_families.ContainsKey(serviceType)) return _families[serviceType];

            lock (_familyLock)
            {
                if (_families.ContainsKey(serviceType)) return _families[serviceType];

                return TryToCreateMissingFamily(serviceType);
            }
        }
        
        public Instance FindDefault(Type serviceType)
        {
            if (serviceType.IsSimple()) return null;
            
            return ResolveFamily(serviceType)?.Default;
        }

        public Instance[] FindAll(Type serviceType)
        {
            return ResolveFamily(serviceType)?.Instances.Values.ToArray() ?? new Instance[0];
        }
        
        public bool CouldBuild(ConstructorInfo ctor)
        {
            return ctor.GetParameters().All(x => ByType.ContainsKey(x.ParameterType) || FindDefault(x.ParameterType) != null || x.IsOptional);
        }

        public bool CouldBuild(Type concreteType)
        {
            var ctor = ConstructorInstance.DetermineConstructor(this, concreteType, out string message);
            return ctor != null && message.IsEmpty();
        }

        public void Dispose()
        {
            foreach (var instance in AllInstances().OfType<IDisposable>())
            {
                instance.SafeDispose();
            }
        }

        private readonly Stack<Instance> _chain = new Stack<Instance>();
        private AssemblyScanner[] _scanners = new AssemblyScanner[0];

        internal void StartingToPlan(Instance instance)
        {
            if (_chain.Contains(instance))
            {
                throw new InvalidOperationException("Bi-directional dependencies detected:" + Environment.NewLine + _chain.Select(x => x.ToString()).Join(Environment.NewLine));
            }
            
            _chain.Push(instance);
        }

        internal void FinishedPlanning()
        {
            _chain.Pop();
        }

        public static ServiceGraph Empty()
        {
            return Scope.Empty().ServiceGraph;
        }

        public static ServiceGraph For(Action<ServiceRegistry> configure)
        {
            var registry = new ServiceRegistry();
            configure(registry);
            
            return new Scope(registry).ServiceGraph;
        }

        public ServiceFamily TryToCreateMissingFamily(Type serviceType)
        {
            // TODO -- will need to make this more formal somehow
            if (serviceType.IsSimple() || serviceType.IsDateTime() || serviceType == typeof(TimeSpan) || serviceType.IsValueType || serviceType == typeof(DateTimeOffset)) return new ServiceFamily(serviceType);
            
            var family = FamilyPolicies.FirstValue(x => x.Build(serviceType, this));
            _families.SmartAdd(serviceType, family);
            
            if (!_inPlanning)
            {
                buildOutMissingResolvers();
            }

            return family;
        }

        IServiceFamilyConfiguration IModel.For<T>()
        {
            return ResolveFamily(typeof(T));
        }

        IServiceFamilyConfiguration IModel.For(Type type)
        {
            return ResolveFamily(type);
        }

        IEnumerable<IServiceFamilyConfiguration> IModel.ServiceTypes => _families.Values.ToArray();

        IEnumerable<Instance> IModel.InstancesOf(Type serviceType)
        {
            return FindAll(serviceType);
        }

        IEnumerable<Instance> IModel.InstancesOf<T>()
        {
            return FindAll(typeof(T));
        }

        Type IModel.DefaultTypeFor<T>()
        {
            return FindDefault(typeof(T))?.ImplementationType;
        }

        Type IModel.DefaultTypeFor(Type serviceType)
        {
            return FindDefault(serviceType)?.ImplementationType;
        }

        IEnumerable<Instance> IModel.AllInstances => AllInstances().ToArray();

        T[] IModel.GetAllPossible<T>()
        {
            return AllInstances().ToArray()
                .Where(x => x.ImplementationType.CanBeCastTo(typeof(T)))
                .Where(x => x.Resolver != null)
                .Select(x => x.Resolver.Resolve(_rootScope))
                .OfType<T>()
                .ToArray();
        }

        bool IModel.HasRegistrationFor(Type serviceType)
        {
            return FindDefault(serviceType) != null;
        }

        bool IModel.HasRegistrationFor<T>()
        {
            return FindDefault(typeof(T)) != null;
        }

        IEnumerable<AssemblyScanner> IModel.Scanners => _scanners;

        internal void ClearPlanning()
        {
            _chain.Clear();
        }

        public bool CouldResolve(Type type)
        {
            return FindDefault(type) != null;
        }

        public static ServiceGraph For(IServiceCollection services)
        {
            return new Scope(services).ServiceGraph;
        }

        public void AppendServices(IServiceCollection services)
        {
            applyScanners(services).Wait(2.Seconds());

            services
                .Where(x => !x.ServiceType.HasAttribute<BlueMilkIgnoreAttribute>())

                .GroupBy(x => x.ServiceType)
                .Each(group =>
                {
                    if (_families.ContainsKey(group.Key))
                    {
                        var family = _families[group.Key];
                        family.Append(group);
                    }
                    else
                    {
                        var family = buildFamilyForInstanceGroup(services, @group);
                        _families.Add(@group.Key, family);
                    }
                });

            buildOutMissingResolvers();

            var serviceTypes = services
                .Select(x => x.ServiceType)
                .Where(x => !x.IsOpenGeneric())
                .Distinct();

            foreach (var serviceType in serviceTypes)
            {
                var family = _families[serviceType];
                
                if (ByType.ContainsKey(family.ServiceType))
                {
                    ByType[serviceType] = family.Default.Resolver;
                }
                else
                {
                    ByType.Add(serviceType, family.Default.Resolver);
                }
            }

        }
    }
}