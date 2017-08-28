using System;
using System.Reflection;
using Baseline;
using Oakton;

namespace Jasper.CommandLine
{
    public static class JasperAgent
    {
        public static int Run(JasperRegistry registry, string[] args = null)
        {
            if (args == null || args.Length == 0)
            {
                args = new string[]{"run"};
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

        public static int Run<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            return Run<T>(null, configure);
        }

        public static int Run(Action<JasperRegistry> configure)
        {
            return Run<JasperRegistry>(null, configure);
        }

        public static int Run<T>(string[] args, Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return Run(registry, args);
        }

        public static int Run(string[] args, Action<JasperRegistry> configure)
        {
            return Run<JasperRegistry>(args, configure);
        }

    }
}
