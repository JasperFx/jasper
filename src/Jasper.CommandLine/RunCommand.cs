using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Baseline;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Runs the configured Jasper application")]
    public class RunCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            var runtime = input.BuildRuntime();

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

            return true;
        }
    }
}
