using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Jasper
{
    public static class JasperAgent
    {
        public static void Run(JasperRegistry registry)
        {
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
                    done.Wait();
                }
            }
            finally
            {
                cts?.Dispose();
            }
        }

        public static void Run<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            Run(registry);
        }

        public static void Run(Action<JasperRegistry> configure)
        {
            Run<JasperRegistry>(configure);
        }

    }
}
