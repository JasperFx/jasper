using System;
using System.Reflection;
using Oakton;

namespace Jasper.CommandLine
{
    public static class JasperAgent
    {
        internal static JasperRegistry Registry { get; set; }


        public static int Run(JasperRegistry registry, string[] args = null)
        {
            args = args ?? new string[]{"run"};

            Registry = registry ?? throw new ArgumentNullException(nameof(registry));


            var executor = buildExecutor(_ => { });

            return executor.Execute(args);
        }

        // TODO -- later, add extensibility into this thing
        private static CommandExecutor buildExecutor(Action<CommandFactory> configure)
        {
            var executor = CommandExecutor.For(factory =>
            {
                factory.RegisterCommands(typeof(RunCommand).GetTypeInfo().Assembly);
                configure(factory);

            });


            return executor;
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
