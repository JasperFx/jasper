using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Internals.Codegen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using StructureMap.Graph;
using ServiceCollectionExtensions = Jasper.Internals.Scanning.Conventions.ServiceCollectionExtensions;

namespace Jasper
{
    /// <summary>
    /// Strictly used to override the ASP.Net Core environment name on bootstrapping
    /// </summary>
    public static class JasperEnvironment
    {
        public static string Name { get; set; }
    }

    public class JasperRuntime : IDisposable
    {
        private readonly JasperRegistry _registry;
        private bool isDisposing;

        private JasperRuntime(JasperRegistry registry, IServiceCollection services)
        {
            services.AddSingleton(this);

            Services = services.ToImmutableArray();

            Container = new Container(_ =>
            {
                _.Populate(services);
            })
            {
                DisposalLock = DisposalLock.Ignore
            };

            registry.Generation.Sources.Add(new NowTimeVariableSource());

            registry.Generation.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            registry.Generation.Assemblies.Add(registry.ApplicationAssembly);

            _registry = registry;

            _bus = new Lazy<IServiceBus>(Get<IServiceBus>);

        }

        /// <summary>
        /// The known service registrations to the underlying IoC container
        /// </summary>
        public ImmutableArray<ServiceDescriptor> Services { get; }

        /// <summary>
        /// The running IWebHost for this applicastion
        /// </summary>
        public IWebHost Host => _registry.Features.For<AspNetCoreFeature>().Host;

        /// <summary>
        /// The main application assembly for the running application
        /// </summary>
        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;

        /// <summary>
        /// The underlying StructureMap container
        /// </summary>
        public IContainer Container { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Summary of all the message handling, subscription, and message publishing
        /// capabilities of the running Jasper application
        /// </summary>
        public ServiceCapabilities Capabilities { get; internal set; }

        void IDisposable.Dispose()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || isDisposing) return;

            isDisposing = true;

            foreach (var feature in _registry.Features)
                feature.Dispose();

            Container.DisposalLock = DisposalLock.Unlocked;
            Container?.Dispose();

            IsDisposed = true;
        }

        /// <summary>
        /// Creates a Jasper application for the current executing assembly
        /// using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static JasperRuntime Basic()
        {
            return bootstrap(new JasperRegistry()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static JasperRuntime For(JasperRegistry registry)
        {
            return bootstrap(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Builds and initializes a JasperRuntime for the JasperRegistry of
        /// type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static JasperRuntime For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static JasperRuntime For(Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure?.Invoke(registry);

            return bootstrap(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Container.GetInstance<T>();
        }

        /// <summary>
        /// Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            return Container.GetInstance(type);
        }


        private static async Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            applyExtensions(registry);

            registry.Settings.Bootstrap();

            var features = registry.Features;

            var serviceRegistries = await Task.WhenAll(features.Select(x => x.Bootstrap(registry)))
                .ConfigureAwait(false);

            var collections = new List<IServiceCollection>();
            collections.AddRange(serviceRegistries);
            collections.Add(registry.ExtensionServices);
            collections.Add(registry.Services);


            var services = await ServiceCollectionExtensions.Combine(collections.ToArray());
            registry.Generation.ReadServices(services);


            var runtime = new JasperRuntime(registry, services);


            await Task.WhenAll(features.Select(x => x.Activate(runtime, registry.Generation)))
                .ConfigureAwait(false);

            return runtime;
        }

        private static void applyExtensions(JasperRegistry registry)
        {
            var assemblies = AssemblyFinder
                .FindAssemblies(a => a.HasAttribute<JasperModuleAttribute>())
                .ToArray();

            if (!assemblies.Any()) return;

            var extensions = assemblies
                .Select(x => x.GetAttribute<JasperModuleAttribute>().ExtensionType)
                .Select(x => Activator.CreateInstance(x).As<IJasperExtension>())
                .ToArray();

            registry.ApplyExtensions(extensions);
        }

        /// <summary>
        /// Writes a textual report about the configured transports and servers
        /// for this application
        /// </summary>
        /// <param name="writer"></param>
        public void Describe(TextWriter writer)
        {
            var hosting = Get<IHostingEnvironment>();
            writer.WriteLine($"Hosting environment: {hosting.EnvironmentName}");
            writer.WriteLine($"Content root path: {hosting.ContentRootPath}");


            foreach (var feature in _registry.Features)
            {
                feature.Describe(this, writer);
            }
        }

        private readonly Lazy<IServiceBus> _bus;

        /// <summary>
        /// Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        public IServiceBus Bus => _bus.Value;

        /// <summary>
        /// The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        public string ServiceName => _registry.ServiceName;
    }
}


