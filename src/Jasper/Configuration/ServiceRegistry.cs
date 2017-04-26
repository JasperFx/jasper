using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace Jasper.Configuration
{
    public class ServiceRegistry : Registry, IServiceCollection
    {
        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public SmartInstance<TImplementation, TService> SetServiceIfNone<TService, TImplementation>()
            where TImplementation : TService
        {
            return For<TService>().UseIfNone<TImplementation>();
        }


        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        public void SetServiceIfNone(Type type, Instance instance)
        {
            For(type).Configure(x => x.Fallback = instance);
        }


        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public ObjectInstance SetServiceIfNone<TService>(TService value)
        {
            return For<TService>().UseIfNone(value);
        }

        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="concreteType"></param>
        /// <returns></returns>
        public Instance SetServiceIfNone(Type interfaceType, Type concreteType)
        {
            var instance = new ConfiguredInstance(concreteType);
            For(interfaceType).Configure(x => x.Fallback = instance);
            return instance;
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public Instance AddService<TService, TImplementation>() where TImplementation : TService
        {
            return For<TService>().Add<TImplementation>();
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementation"></param>
        /// <returns></returns>
        public Instance AddService<TService>(Type implementationType)
        {
            var instance = new ConfiguredInstance(implementationType);

            For<TService>().AddInstance(instance);

            return instance;
        }

        /// <summary>
        /// Registers a instanceault implementation for a service.  Overwrites any existing
        /// registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public SmartInstance<TImplementation, TService> ReplaceService<TService, TImplementation>()
            where TImplementation : TService
        {
            return For<TService>().ClearAll().Use<TImplementation>();
        }

        /// <summary>
        /// Registers a instanceault implementation for a service.  Overwrites any existing
        /// registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public ObjectInstance<TService, TService> ReplaceService<TService>(TService value) where TService : class
        {
            return For<TService>().ClearAll().Use(value);
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public void AddService<TService>(TService value) where TService : class
        {
            For<TService>().Add(value);
        }

        /// <summary>
        /// Removes any registrations for type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddService(Type type, Instance instance)
        {
            For(type).Add(instance);
        }

        public void ReplaceService(Type type, Instance @default)
        {
            For(type).ClearAll();
            For(type).Use(@default);
        }

        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

        /// <inheritdoc />
        public int Count => _descriptors.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public ServiceDescriptor this[int index]
        {
            get
            {
                return _descriptors[index];
            }
            set
            {
                _descriptors[index] = value;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _descriptors.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item)
        {
            return _descriptors.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _descriptors.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item)
        {
            return _descriptors.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            _descriptors.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return _descriptors.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _descriptors.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _descriptors.RemoveAt(index);
        }

        internal void BakeAspNetCoreServices()
        {
            this.Populate(_descriptors);
        }
    }

    public static class ContainerExtensions
    {
        /// <summary>
        /// Populates the container using the specified service descriptors.
        /// </summary>
        /// <remarks>
        /// This method should only be called once per container.
        /// </remarks>
        /// <param name="container">The container.</param>
        /// <param name="descriptors">The service descriptors.</param>
        public static void Populate(this IContainer container, IEnumerable<ServiceDescriptor> descriptors)
        {
            container.Configure(config => config.Populate(descriptors));
        }

        /// <summary>
        /// Populates the container using the specified service descriptors.
        /// </summary>
        /// <remarks>
        /// This method should only be called once per container.
        /// </remarks>
        /// <param name="config">The configuration.</param>
        /// <param name="descriptors">The service descriptors.</param>
        public static void Populate(this ConfigurationExpression config, IEnumerable<ServiceDescriptor> descriptors)
        {
            Populate((Registry)config, descriptors);
        }

        /// <summary>
        /// Populates the registry using the specified service descriptors.
        /// </summary>
        /// <remarks>
        /// This method should only be called once per container.
        /// </remarks>
        /// <param name="registry">The registry.</param>
        /// <param name="descriptors">The service descriptors.</param>
        public static void Populate(this Registry registry, IEnumerable<ServiceDescriptor> descriptors)
        {
            // HACK: We insert this action in order to prevent Populate being called twice on the same container.
            registry.Configure(ThrowIfMarkerInterfaceIsRegistered);

            registry.For<IMarkerInterface>();

            registry.Policies.ConstructorSelector<AspNetConstructorSelector>();

            registry.For<IServiceProvider>()
                .LifecycleIs(Lifecycles.Container)
                .Use<StructureMapServiceProvider>();

            registry.For<IServiceScopeFactory>()
                .LifecycleIs(Lifecycles.Container)
                .Use<StructureMapServiceScopeFactory>();

            registry.Register(descriptors);
        }

        internal class AspNetConstructorSelector : IConstructorSelector
        {
            // ASP.NET expects registered services to be considered when selecting a ctor, SM doesn't by default.
            public ConstructorInfo Find(Type pluggedType, DependencyCollection dependencies, PluginGraph graph) =>
                pluggedType.GetTypeInfo()
                    .DeclaredConstructors
                    .Select(ctor => new { Constructor = ctor, Parameters = ctor.GetParameters() })
                    .Where(x => x.Parameters.All(param => graph.HasFamily(param.ParameterType) || dependencies.Any(dep => dep.Type == param.ParameterType)))
                    .OrderByDescending(x => x.Parameters.Length)
                    .Select(x => x.Constructor)
                    .FirstOrDefault();
        }

        private static void ThrowIfMarkerInterfaceIsRegistered(PluginGraph graph)
        {
            if (graph.HasFamily<IMarkerInterface>())
            {
                throw new InvalidOperationException("Populate should only be called once per container.");
            }
        }

        private static void Register(this IProfileRegistry registry, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                registry.Register(descriptor);
            }
        }

        private static void Register(this IProfileRegistry registry, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.ImplementationType);

                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.CreateFactory());

                return;
            }

            registry.For(descriptor.ServiceType)
                .LifecycleIs(descriptor.Lifetime)
                .Use(descriptor.ImplementationInstance);
        }

        internal static Expression<Func<IContext, object>> CreateFactory(this ServiceDescriptor descriptor)
        {
            return context => descriptor.ImplementationFactory(context.GetInstance<IServiceProvider>());
        }

        private interface IMarkerInterface { }


    }

    internal static class HelperExtensions
    {
        public static bool IsGenericEnumerable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static GenericFamilyExpression LifecycleIs(this GenericFamilyExpression instance, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return instance.LifecycleIs(Lifecycles.Singleton);
                case ServiceLifetime.Scoped:
                    return instance.LifecycleIs(Lifecycles.Container);
                case ServiceLifetime.Transient:
                    return instance.LifecycleIs(Lifecycles.Unique);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }

        public static bool HasFamily<TPlugin>(this PluginGraph graph)
        {
            return graph.HasFamily(typeof(TPlugin));
        }
    }

    public sealed class StructureMapServiceProvider : IServiceProvider, ISupportRequiredService
    {
        public StructureMapServiceProvider(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public object GetService(Type serviceType)
        {
            if (serviceType.IsGenericEnumerable())
            {
                // Ideally we'd like to call TryGetInstance here as well,
                // but StructureMap does't like it for some weird reason.
                return GetRequiredService(serviceType);
            }

            return Container.TryGetInstance(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }
    }

    internal sealed class StructureMapServiceScopeFactory : IServiceScopeFactory
    {
        public StructureMapServiceScopeFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IServiceScope CreateScope()
        {
            return new StructureMapServiceScope(Container.GetNestedContainer());
        }

        private class StructureMapServiceScope : IServiceScope
        {
            public StructureMapServiceScope(IContainer container)
            {
                Container = container;
                ServiceProvider = container.GetInstance<IServiceProvider>();
            }

            private IContainer Container { get; }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose() => Container.Dispose();
        }
    }



    public class StructureMapServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public StructureMapServiceProviderFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            var registry = new Registry();
            foreach (var service in services)
            {
                register(registry, service);
            }

            Container.Configure(_ => _.AddRegistry(registry));

            return Container;
        }

        private static void register(Registry registry, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.ImplementationType);

                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.CreateFactory());

                return;
            }

            registry.For(descriptor.ServiceType)
                .LifecycleIs(descriptor.Lifetime)
                .Use(descriptor.ImplementationInstance);
        }

        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return new StructureMapServiceProvider(container);
        }
    }

}
