using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Runs the configured Jasper application")]
    public class RunCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            var host = input.BuildHost(StartMode.Full);

            var done = new ManualResetEventSlim(false);
            var cts = new CancellationTokenSource();

            try
            {
                void shutdown()
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        host.Dispose();
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

                using (host)
                {
                    host.Describe(Console.Out);

                    Console.WriteLine("Running all environment checks...");
                    host.ExecuteAllEnvironmentChecks();

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
