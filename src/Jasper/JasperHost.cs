using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Jasper
{
    /// <summary>
    /// Used to bootstrap a Jasper application
    /// </summary>
    public static class JasperHost
    {
        /// <summary>
        /// Bootstrap and run the given JasperRegistry
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int Run(JasperRegistry registry, string[] args = null)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            var runtimeSource = CreateDefaultBuilder().UseJasper(registry);

            return Execute(runtimeSource, registry.ApplicationAssembly, args);
        }

        internal static int Execute(IWebHostBuilder runtimeSource, Assembly applicationAssembly, string[] args)
        {
            if (args == null || args.Length == 0 || args[0].StartsWith("-"))
                args = new[] {"run"}.Concat(args ?? new string[0]).ToArray();

            if (applicationAssembly == null)
            {
                var name = runtimeSource.GetSetting(WebHostDefaults.ApplicationKey);
                if (name.IsNotEmpty())
                {
                    applicationAssembly = Assembly.Load(name);
                }
            }

            return buildExecutor(runtimeSource, applicationAssembly).Execute(args);
        }


        private static CommandExecutor buildExecutor(IWebHostBuilder source, Assembly applicationAssembly)
        {
            return CommandExecutor.For(factory =>
            {


                factory.RegisterCommands(typeof(RunCommand).GetTypeInfo().Assembly);
                if (applicationAssembly != null) factory.RegisterCommands(applicationAssembly);

                foreach (var assembly in FindExtensionAssemblies(applicationAssembly)) factory.RegisterCommands(assembly);

                factory.ConfigureRun = cmd =>
                {
                    if (cmd.Input is JasperInput) cmd.Input.As<JasperInput>().WebHostBuilder = source;
                };
            });
        }


        /// <summary>
        /// Bootstrap and run a Jasper application with customizations
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Run<T>(string[] args, Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return Run(registry, args);
        }

        /// <summary>
        /// Bootstrap and run a Jasper application with customizations
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static int Run(string[] args, Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure(registry);
            return Run(registry, args);
        }

        /// <summary>
        /// Bootstrap and run a basic Jasper application for this assemblhy
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int RunBasic(string[] args)
        {
            return Run(new JasperRegistry(), args);
        }


        /// <summary>
        /// Bootstrap and run a Jasper application for the configured IWebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int Run(IWebHostBuilder hostBuilder, string[] args)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
            return Execute(hostBuilder, null, args);
        }




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
    }
}
