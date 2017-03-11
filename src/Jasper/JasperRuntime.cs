using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;

namespace Jasper
{
    public class JasperRuntime : IDisposable
    {
        public Assembly ApplicationAssembly { get; }
        private readonly JasperRegistry _registry;

        private JasperRuntime(JasperRegistry registry, Registry[] serviceRegistries, Assembly applicationAssembly)
        {
            ApplicationAssembly = applicationAssembly;
            _registry = registry;

            Container = new Container(_ =>
            {
                _.AddRegistry(registry.Services);
                foreach (var serviceRegistry in serviceRegistries)
                {
                    _.AddRegistry(serviceRegistry);
                }
            })
            {
                DisposalLock = DisposalLock.Ignore
            };

        }

        public static JasperRuntime Basic()
        {
            var assembly = findTheCallingAssembly();
            return bootstrap(assembly, new JasperRegistry()).GetAwaiter().GetResult();
        }

        public static JasperRuntime For(JasperRegistry registry)
        {
            var assembly = registry.GetType().GetTypeInfo().Assembly;
            if (assembly == typeof(JasperRuntime).GetTypeInfo().Assembly)
            {
                assembly = findTheCallingAssembly();
            }

            return bootstrap(assembly, registry).GetAwaiter().GetResult();
        }

        public static JasperRuntime For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            var assembly = typeof(T).GetTypeInfo().Assembly;

            return bootstrap(assembly, registry).GetAwaiter().GetResult();
        }


        public IJasperRegistry Registry => _registry;

        private static Assembly findTheCallingAssembly()
        {
            string trace = Environment.StackTrace;

            var parts = trace.Split('\n');
            var candidate = parts[4].Trim().Substring(3);

            Assembly assembly = null;
            var names = candidate.Split('.');
            for (var i = names.Length - 2; i > 0; i--) {
                var possibility = string.Join(".", names.Take(i).ToArray());

                try
                {

                    assembly = System.Reflection.Assembly.Load(new AssemblyName(possibility));
                    break;
                }
                catch (Exception e)
                {
                    // Nothing
                }
            }

            return assembly;
        }

        private async static Task<JasperRuntime> bootstrap(Assembly appAssembly, JasperRegistry registry)
        {
            /*
Questions:
* Pre-load settings that we know we're looking for?


1. Look for extensions
2. Apply all extensions
3. Do all the necessary Settings stuff
4. Activate each feature in parallel
5. Apply services
6. Activate each in parallel



*/

            var assemblies = AssemblyFinder.FindAssemblies(a => a.HasAttribute<JasperModuleAttribute>());


            await applyExtensions(registry, assemblies).ConfigureAwait(false);

            // TODO -- apply all the settings alterations

            var features = registry.Features;

            var serviceRegistries = await Task.WhenAll(features.Select(x => x.Bootstrap(registry))).ConfigureAwait(false);

            var runtime = new JasperRuntime(registry, serviceRegistries, appAssembly);

            await Task.WhenAll(features.Select(x => x.Activate(runtime))).ConfigureAwait(false);

            return runtime;
        }

        public IContainer Container { get;}

        private static async Task applyExtensions(JasperRegistry registry, IEnumerable<Assembly> assemblies)
        {
            Func<Type, bool> filter = type => type.CanBeCastTo<IJasperExtension>() && type.IsConcreteWithDefaultCtor();

            var extensionTypes = await TypeRepository.FindTypes(assemblies,
                TypeClassification.Concretes | TypeClassification.Closed, filter).ConfigureAwait(false);

            foreach (var extensionType in extensionTypes)
            {
                var extension = Activator.CreateInstance(extensionType).As<IJasperExtension>();
                extension.Configure(registry);
            }
        }

        public void Dispose()
        {
            foreach (var feature in _registry.Features)
            {
                feature.Dispose();
            }

            Container.DisposalLock = DisposalLock.Unlocked;
            Container?.Dispose();
        }
    }
}