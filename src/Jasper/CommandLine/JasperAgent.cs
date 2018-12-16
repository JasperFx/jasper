using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Oakton;

namespace Jasper.CommandLine
{
    /// <summary>
    ///     Used to quickly turn a Jasper application into a console application
    ///     with administration commands
    /// </summary>
    public static class JasperAgent
    {
        /// <summary>
        ///     Bootstrap and run the given JasperRegistry
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int Run(JasperRegistry registry, string[] args = null)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            var runtimeSource = new RegistryRuntimeSource(registry);

            return Execute(runtimeSource, args);
        }

        internal static int Execute(IRuntimeSource runtimeSource, string[] args)
        {
            if (args == null || args.Length == 0 || args[0].StartsWith("-"))
                args = new[] {"run"}.Concat(args ?? new string[0]).ToArray();


            return buildExecutor(runtimeSource).Execute(args);
        }


        private static CommandExecutor buildExecutor(IRuntimeSource source)
        {
            return CommandExecutor.For(factory =>
            {
                factory.RegisterCommands(typeof(RunCommand).GetTypeInfo().Assembly);
                if (source.ApplicationAssembly != null) factory.RegisterCommands(source.ApplicationAssembly);

                foreach (var assembly in JasperRuntime.FindExtensionAssemblies()) factory.RegisterCommands(assembly);

                factory.ConfigureRun = cmd =>
                {
                    if (cmd.Input is JasperInput) cmd.Input.As<JasperInput>().Source = source;
                };
            });
        }

        /// <summary>
        ///     Bootstrap and run a Jasper application with customizations
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
        ///     Bootstrap and run a Jasper application with customizations
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static int Run(string[] args, Action<JasperRegistry> configure)
        {
            return Run<JasperRegistry>(args, configure);
        }

        /// <summary>
        ///     Bootstrap and run a basic Jasper application for this assemblhy
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
            return Execute(new WebHostBuilderRuntimeSource(hostBuilder), args);
        }

        /// <summary>
        /// Syntactical sugar to execute the Jasper command line for a configured WebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int RunJasper(this IWebHostBuilder hostBuilder, string[] args)
        {
            return Run(hostBuilder, args);
        }
    }

    public enum StartMode
    {
        /// <summary>
        /// Completely builds and starts the underlying IWebHost
        /// </summary>
        Full,

        /// <summary>
        /// Builds, but does not start the underlying IWebHost. Suitable for diagnostic commands
        /// </summary>
        Lightweight
    }

    public interface IRuntimeSource
    {
        JasperRuntime Lightweight();
        JasperRuntime Full();

        IWebHostBuilder HostBuilder { get; }
        Assembly ApplicationAssembly { get; }
    }

    public class RegistryRuntimeSource : IRuntimeSource
    {
        private readonly JasperRegistry _registry;

        public RegistryRuntimeSource(JasperRegistry registry)
        {
            _registry = registry;
        }

        public IWebHostBuilder HostBuilder => _registry.Hosting;
        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;

        public JasperRuntime Lightweight()
        {
            return JasperRuntime.Lightweight(_registry);
        }

        public JasperRuntime Full()
        {
            return JasperRuntime.For(_registry);
        }
    }

    public class WebHostBuilderRuntimeSource : IRuntimeSource
    {
        public WebHostBuilderRuntimeSource(IWebHostBuilder builder)
        {
            HostBuilder = builder;
            ApplicationAssembly = Assembly.Load(builder.GetSetting(WebHostDefaults.ApplicationKey));
        }

        public JasperRuntime Lightweight()
        {
            var host = HostBuilder.Build();
            return new JasperRuntime(host);
        }

        public JasperRuntime Full()
        {
            var host = HostBuilder.Start();
            return new JasperRuntime(host);
        }

        public Assembly ApplicationAssembly { get; }

        public IWebHostBuilder HostBuilder { get; }
    }
}
