using Jasper;
using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Oakton;

[assembly:JasperFeature]

namespace Jasper.CommandLine
{
    /// <summary>
    /// Used to quickly turn a Jasper application into a console application
    /// with administration commands
    /// </summary>
    public static class JasperAgent
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
            if (args == null || args.Length == 0 || args[0].StartsWith("-"))
            {
                args = new string[]{"run"}.Concat(args).ToArray();
            }


            if (registry == null) throw new ArgumentNullException(nameof(registry));


            return buildExecutor(registry).Execute(args);
        }

        // TODO -- later, add extensibility into this thing
        private static CommandExecutor buildExecutor(JasperRegistry registry)
        {
            return CommandExecutor.For(factory =>
            {
                factory.RegisterCommands(typeof(RunCommand).GetTypeInfo().Assembly);
                factory.ConfigureRun = cmd =>
                {
                    if (cmd.Input is JasperInput)
                    {
                        cmd.Input.As<JasperInput>().Registry = registry;
                    }
                };

            });
        }

        /// <summary>
        /// Bootstrap and run a Jasper application with customizations
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Run<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            return Run<T>(null, configure);
        }

        /// <summary>
        /// Bootstrap and run a Jasper application with customizations
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static int Run(Action<JasperRegistry> configure)
        {
            return Run<JasperRegistry>(null, configure);
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
            return Run<JasperRegistry>(args, configure);
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
    }
}
