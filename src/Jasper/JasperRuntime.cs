using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;

namespace Jasper
{
    public class JasperRuntime : IDisposable
    {
        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;
        private readonly JasperRegistry _registry;

        private JasperRuntime(JasperRegistry registry, Registry[] serviceRegistries)
        {
            Container = new Container(_ =>
            {
                _.AddRegistry(registry.Services);
                foreach (var serviceRegistry in serviceRegistries)
                {
                    _.AddRegistry(serviceRegistry);
                }

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


        public IJasperRegistry Registry => _registry;



        private static async Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            var assemblies = AssemblyFinder.FindAssemblies(a => a.HasAttribute<JasperModuleAttribute>());


            await applyExtensions(registry, assemblies).ConfigureAwait(false);

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

        public IContainer Container { get;}

        private static async Task applyExtensions(JasperRegistry registry, IEnumerable<Assembly> assemblies)
        {
            if (!assemblies.Any()) return;

            Func<Type, bool> filter = type => type.CanBeCastTo<IJasperExtension>() && type.IsConcreteWithDefaultCtor();

            var extensionTypes = await TypeRepository.FindTypes(assemblies,
                TypeClassification.Concretes | TypeClassification.Closed, filter).ConfigureAwait(false);

            foreach (var extensionType in extensionTypes)
            {
                var extension = Activator.CreateInstance(extensionType).As<IJasperExtension>();
                extension.Configure(registry);
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;

            foreach (var feature in _registry.Features)
            {
                feature.Dispose();
            }

            Container.DisposalLock = DisposalLock.Unlocked;
            Container?.Dispose();

            IsDisposed = true;
        }
    }
}