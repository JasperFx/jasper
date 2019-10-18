using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using BaselineTypeDiscovery;
using Jasper.Configuration;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace Jasper
{
    /// <summary>
    ///     Used to bootstrap a Jasper application
    /// </summary>
    public static class JasperHost
    {
        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static IJasperHost Basic()
        {
            return bootstrap(new JasperRegistry());
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IJasperHost For(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the JasperRegistry of
        ///     type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static IJasperHost For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IJasperHost For(Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure(registry);
            return bootstrap(registry);
        }

        internal static void ApplyExtensions(JasperRegistry registry)
        {
            var assemblies = FindExtensionAssemblies(registry.ApplicationAssembly);

            if (!assemblies.Any())
            {
                Console.WriteLine("No Jasper extensions are detected");
                return;
            }

            var extensions = assemblies.Select(x => x.GetAttribute<JasperModuleAttribute>().ExtensionType)
                .Where(x => x != null)
                .Select(x => Activator.CreateInstance(x).As<IJasperExtension>())
                .ToArray();


            if (extensions.Any())
            {
                Console.WriteLine($"Found and applying {extensions.Length} Jasper extension(s)");
                foreach (var extension in extensions) Console.WriteLine($"Applying {extension}");
            }
            else
            {
                Console.WriteLine("No Jasper extensions are detected");
            }

            registry.ApplyExtensions(extensions);
        }

        private static Assembly[] _extensions;

        internal static Assembly[] FindExtensionAssemblies(Assembly applicationAssembly)
        {
            if (_extensions != null) return _extensions;

            _extensions = AssemblyFinder
                .FindAssemblies(txt => { }, false)
                .Concat(AppDomain.CurrentDomain.GetAssemblies())
                .Distinct()
                .Where(a => a.HasAttribute<JasperModuleAttribute>())
                .ToArray();

            var names = _extensions.Select(x => x.GetName().Name);

            Assembly[] FindDependencies(Assembly a) => _extensions.Where(x => names.Contains(x.GetName().Name)).ToArray();


            _extensions = _extensions.TopologicalSort(FindDependencies, false).ToArray();

            return _extensions;
        }

        private static JasperRuntime bootstrap(JasperRegistry registry)
        {
            var host = Host.CreateDefaultBuilder()
                .UseJasper(registry)
                .Start();


            return new JasperRuntime(host);
        }


        /// <summary>
        ///     Shortcut to create a new empty WebHostBuilder with Jasper's default
        ///     settings, add the JasperRegistry, and bootstrap the application
        ///     from the command line
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Task<int> Run<T>(string[] args) where T : JasperRegistry, new()
        {
            return Host.CreateDefaultBuilder().UseJasper<T>().RunOaktonCommands(args);
        }

        /// <summary>
        ///     Shortcut to create a new empty WebHostBuilder with Jasper's default
        ///     settings, add Jasper with the supplied configuration, and bootstrap the application
        ///     from the command line
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Task<int> Run(string[] args, Action<JasperRegistry> configure)
        {
            return Host.CreateDefaultBuilder().UseJasper(configure).RunOaktonCommands(args);
        }
    }
}
