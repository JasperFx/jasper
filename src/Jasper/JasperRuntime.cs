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

        public ImmutableArray<ServiceDescriptor> Services { get; }

        public IWebHost Host => _registry.Features.For<AspNetCoreFeature>().Host;

        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;

        public IContainer Container { get; }

        public bool IsDisposed { get; private set; }
        public ServiceCapabilities Capabilities { get; internal set; }

        public void Dispose()
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

        public static JasperRuntime Basic()
        {
            return bootstrap(new JasperRegistry()).GetAwaiter().GetResult();
        }

        public static JasperRuntime For(JasperRegistry registry)
        {
            return bootstrap(registry).GetAwaiter().GetResult();
        }

        public static JasperRuntime For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry).GetAwaiter().GetResult();
        }

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

        public IServiceBus Bus => _bus.Value;

        public string ServiceName => _registry.ServiceName;
    }
}


