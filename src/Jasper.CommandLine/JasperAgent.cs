using System;

namespace Jasper.CommandLine
{
    public static class JasperAgent
    {
        internal static JasperRegistry Registry { get; set; }


        public static void Run(JasperRegistry registry, string[] args = null)
        {
            args = args ?? new string[0];

            Registry = registry ?? throw new ArgumentNullException(nameof(registry));

            if (args.Length == 0)
            {
                new RunCommand().Execute(new RunInput());
            }
            else
            {
                throw new NotImplementedException("Send it through Oakton");
            }
        }

        public static void Run<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            Run<T>(null, configure);
        }

        public static void Run(Action<JasperRegistry> configure)
        {
            Run<JasperRegistry>(null, configure);
        }

        public static void Run<T>(string[] args, Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            Run(registry, args);
        }

        public static void Run(string[] args, Action<JasperRegistry> configure)
        {
            Run<JasperRegistry>(args, configure);
        }

    }
}
