using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Consul.Internal;
using Xunit;

namespace Jasper.Consul.Testing
{
    [CollectionDefinition("Consul")]
    public class DatabaseCollection : ICollectionFixture<ConsulServer>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class ConsulServer : IDisposable
    {
        private static Process _process;

        public ConsulServer()
        {
            spinUp().Wait(5.Seconds());
        }

        public void Dispose()
        {
            _process?.Kill();
        }

        private static async Task spinUp()
        {
// First, ping to see if consul is running.

            var isRunning = false;

            try
            {
                using (var gateway = new ConsulGateway(new ConsulSettings()))
                {
                    await gateway.SetProperty("ping", "value");
                    isRunning = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("No Consul agent found, proceeding to start one up");
            }

            if (isRunning) return;


            var basePath = AppContext.BaseDirectory;
            while (!File.Exists(basePath.AppendPath("ConsulServer.cs")))
                basePath = basePath.ParentDirectory();

            string executable = null;

            if (Platform.IsWindows)
                executable = basePath.AppendPath("windows", "consul.exe");
            else if (Platform.IsDarwin)
                executable = basePath.AppendPath("osx", "consul");
            else
                throw new InvalidOperationException(
                    "Sorry, we only support Windows or OSX unless you spin up Consul externally");


            _process = Process.Start(executable, "agent -dev");

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (var gateway = new ConsulGateway(new ConsulSettings()))
                    {
                        await gateway.SetProperty("ping", "value");
                        break;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }

        }
    }
}
