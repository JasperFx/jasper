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
            
            addScopeResolver<Scope>(services);
            addScopeResolver<IServiceProvider>(services);
            addScopeResolver<IContainer>(services);
            addScopeResolver<IServiceScopeFactory>(services);
            
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

            var generatedSingletons = AllInstances().OfType<GeneratedInstance>().Where(x => x.Lifetime != ServiceLifetime.Transient && !x.ServiceType.IsOpenGeneric()).ToArray();
            if (generatedSingletons.Any())
            {
                var assembly = ToGeneratedAssembly();
                foreach (var instance in generatedSingletons)
                {
                    instance.GenerateResolver(assembly);
                }
                
                assembly.CompileAll();

                foreach (var instance in generatedSingletons)
                {
                    instance.AttachResolver(_rootScope);
                }
            }

        }
        



        private void buildOutMissingResolvers()
        {
            if (_inPlanning) return;

            _inPlanning = true;

            try
            {
                planResolutionStrategies();
            }
            finally
            {
                _inPlanning = false;
            }
        }


        internal GeneratedAssembly ToGeneratedAssembly()
        {
            // TODO -- will need to get at the GenerationRules from somewhere
            var generatedAssembly = new GeneratedAssembly(new GenerationRules("Jasper.Generated"));
            AllInstances().SelectMany(x => x.ReferencedAssemblies())
                .Distinct()
                .Each(a => generatedAssembly.Generation.Assemblies.Fill(a));
            return generatedAssembly;
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
            return _families.Values.ToArray().SelectMany(x => x.All).ToArray();
        }

        public IReadOnlyDictionary<Type, ServiceFamily> Families => _families;

        public bool HasFamily(Type serviceType)
        {
            return _families.ContainsKey(serviceType);
        }

        public Instance FindInstance(Type serviceType, string name)
        {
            return ResolveFamily(serviceType).InstanceFor(name);
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
            return ResolveFamily(serviceType)?.All ?? new Instance[0];
        }
        
        public bool CouldBuild(ConstructorInfo ctor)
        {
            return ctor.GetParameters().All(x => FindDefault(x.ParameterType) != null || x.IsOptional);
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
                .Select(x => x.Resolve(_rootScope))
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

        }
    }
}