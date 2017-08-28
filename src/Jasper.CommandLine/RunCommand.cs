using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Baseline;
using Oakton;

namespace Jasper.CommandLine
{
    public class RunInput
    {
        [Description("Use to override the ASP.Net Environment name")]
        public string EnvironmentFlag { get; set; }

        [Description("Write out much more information at startup and enables console logging")]
        public bool VerboseFlag { get; set; }
    }

    public class RunCommand : OaktonCommand<RunInput>
    {
        public override bool Execute(RunInput input)
        {
            if (input.VerboseFlag)
            {
                Console.WriteLine("Verbose flag is on.");
                JasperAgent.Registry.Logging.UseConsoleLogging = true;

            }

            if (input.EnvironmentFlag.IsNotEmpty())
            {
                JasperAgent.Registry.EnvironmentName = input.EnvironmentFlag;
                Console.WriteLine($"Overriding the Environment Name to '{input.EnvironmentFlag}'");
            }


            var runtime = JasperRuntime.For(JasperAgent.Registry);

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
