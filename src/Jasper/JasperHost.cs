using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.CommandLine;
using Jasper.Configuration;
using Jasper.Http;
using Lamar.Scanning.Conventions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oakton;
using Oakton.AspNetCore;

namespace Jasper
{
    /// <summary>
    /// Used to bootstrap a Jasper application
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
                foreach (var extension in extensions)
                {
                    Console.WriteLine($"Applying {extension}");
                }
            }
            else
            {
                Console.WriteLine("No Jasper extensions are detected");
            }

            registry.ApplyExtensions(extensions);
        }

        internal static Assembly[] FindExtensionAssemblies(Assembly applicationAssembly)
        {
            return AssemblyFinder
                .FindAssemblies(txt => { }, false)
                .Concat(AppDomain.CurrentDomain.GetAssemblies())
                .Distinct()
                .Where(a => a.HasAttribute<JasperModuleAttribute>())
                .ToArray();
        }

        private static JasperRuntime bootstrap(JasperRegistry registry)
        {
            var host = CreateDefaultBuilder()
                .UseJasper(registry)
                .Start();


            return new JasperRuntime(host);
        }

        /// <summary>
        /// Builds a default, "headless" WebHostBuilder with minimal configuration including
        /// the usage of appsettings.json binding, Debug/Console logging, but without Kestrel
        /// or any other middleware configured
        /// </summary>
        /// <returns></returns>
        public static IWebHostBuilder CreateDefaultBuilder()
        {
            // SAMPLE: default-configuration-options
            return new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();

                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();

                })
                .ConfigureServices(s =>
                {
                    // Registers an empty startup if there is none in the application
                    if (s.All(x => x.ServiceType != typeof(IStartup)))
                    {
                        s.AddSingleton<IStartup>(new NulloStartup());
                    }

                    // Registers a "nullo" server if there is none in the application
                    // i.e., Kestrel isn't applied
                    if (s.All(x => x.ServiceType != typeof(IServer)))
                    {
                        s.AddSingleton<IServer>(new NulloServer());
                    }

                    // This guarantees that the Jasper middleware is part of the RequestDelegate
                    // at the end if it has not been explicitly added
                    s.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter());
                });
            // ENDSAMPLE
        }

        /// <summary>
        /// Shortcut to create a new empty WebHostBuilder with Jasper's default
        /// settings, add the JasperRegistry, and bootstrap the application
        /// from the command line
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Task<int> Run<T>(string[] args) where T : JasperRegistry, new()
        {
            return CreateDefaultBuilder().UseJasper<T>().RunOaktonCommands(args);
        }

        /// <summary>
        /// Shortcut to create a new empty WebHostBuilder with Jasper's default
        /// settings, add Jasper with the supplied configuration, and bootstrap the application
        /// from the command line
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Task<int> Run(string[] args, Action<JasperRegistry> configure)
        {
            return CreateDefaultBuilder().UseJasper(configure).RunOaktonCommands(args);
        }
    }
}
