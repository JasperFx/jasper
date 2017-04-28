using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace Jasper.Configuration
{
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

        public static void Register(this IProfileRegistry registry, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                registry.Register(descriptor);
            }
        }

        public static void Register(this IProfileRegistry registry, ServiceDescriptor descriptor)
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
}