using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Alba;
using JasperHttpTesting;

namespace Jasper.Http.Testing.ContentHandling
{
    public static class HttpTesting
    {
        private static object _locker = new object();
        public static string RootUrl = "http://localhost:5666";

        private static JasperRuntime _runtime;

        static HttpTesting()
        {
            AssemblyLoadContext.Default.Unloading += context =>
            {
                _runtime?.Dispose();
            };
        }

        public static JasperRuntime Runtime => _runtime;

        public static async Task<IScenarioResult> Scenario(Action<Scenario> configure)
        {
            if (_runtime == null)
            {
                // Okay if it's rebuilt here. Better than locking issues
                _runtime = await JasperRuntime.ForAsync<HttpTestingApp>();
            }

            return await _runtime.Scenario(configure);
        }
    }
}
