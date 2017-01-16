using System;
using System.Collections.Generic;
using System.Reflection;
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

        public JasperRuntime(JasperRegistry registry)
        {
            Container = new Container(registry.Services);
            Container.DisposalLock = DisposalLock.Ignore;
        }

        public static JasperRuntime Basic()
        {
            var assembly = determineTheCallingAssembly();
            return bootstrap(assembly, new JasperRegistry()).GetAwaiter().GetResult();
        }

        public static JasperRuntime For(JasperRegistry registry)
        {
            var assembly = registry.GetType().GetTypeInfo().Assembly;
            if (assembly == typeof(JasperRuntime).GetTypeInfo().Assembly)
            {
                assembly = determineTheCallingAssembly();
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

        private static Assembly determineTheCallingAssembly()
        {
            return null;
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

            var runtime = new JasperRuntime(registry);

            return runtime;
        }

        public Container Container { get;}

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
            Container.DisposalLock = DisposalLock.Unlocked;
            Container?.Dispose();
        }
    }
}