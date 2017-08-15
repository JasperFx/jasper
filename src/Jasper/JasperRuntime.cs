using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using Microsoft.AspNetCore.Hosting;
using StructureMap;
using StructureMap.Graph;

namespace Jasper
{
    public class JasperRuntime : IDisposable
    {
        private readonly JasperRegistry _registry;
        private bool isDisposing;

        private JasperRuntime(JasperRegistry registry, Registry[] serviceRegistries)
        {
            Container = new Container(_ =>
            {
                _.AddRegistry(registry.ExtensionServices);
                _.AddRegistry(registry.Services);
                foreach (var serviceRegistry in serviceRegistries)
                    _.AddRegistry(serviceRegistry);

                _.For<JasperRuntime>().Use(this);
            })
            {
                DisposalLock = DisposalLock.Ignore
            };

            registry.Generation.Sources.Add(new StructureMapServices(Container));
            registry.Generation.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            registry.Generation.Assemblies.Add(registry.ApplicationAssembly);

            _registry = registry;
        }

        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;

        public IContainer Container { get; }

        public bool IsDisposed { get; private set; }

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


        private static async Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            applyExtensions(registry);

            registry.Services.BakeAspNetCoreServices();
            registry.Settings.Bootstrap();

            var features = registry.Features;

            var serviceRegistries = await Task.WhenAll(features.Select(x => x.Bootstrap(registry)))
                .ConfigureAwait(false);

            var runtime = new JasperRuntime(registry, serviceRegistries);


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
    }
}


