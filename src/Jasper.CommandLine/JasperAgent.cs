using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Jasper.CommandLine
{
    public static class JasperAgent
    {
        public static void Run(JasperRegistry registry, string[] args = null)
        {
            args = args ?? new string[0];

            var runtime = JasperRuntime.For(registry);

            var done = new ManualResetEventSlim(false);
            var cts = new CancellationTokenSource();

            try
            {
                void shutdown()
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        runtime.Dispose();
                        cts.Cancel();
                    }

                    done.Set();
                }

                var assembly = typeof(JasperRuntime).GetTypeInfo().Assembly;
                AssemblyLoadContext.GetLoadContext(assembly).Unloading += context => shutdown();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    shutdown();
                    eventArgs.Cancel = true;
                };

                using (runtime)
                {
                    runtime.Describe(Console.Out);

                    Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                    done.Wait(cts.Token);
                }
            }
            finally
            {
                cts?.Dispose();
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
