using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Alba;
using JasperHttpTesting;

namespace Jasper.Http.Testing.ContentHandling
{
    public static class HttpTesting
    {
        public static string RootUrl = "http://localhost:5666";

        private static readonly JasperRuntime _runtime;

        static HttpTesting()
        {
            _runtime = JasperRuntime.For<HttpTestingApp>();

            AssemblyLoadContext.Default.Unloading += context =>
            {
                _runtime.Dispose();
            };
        }

        public static JasperRuntime Runtime => _runtime;

        public static Task<IScenarioResult> Scenario(Action<Scenario> configure)
        {
            return _runtime.Scenario(configure);
        }
    }
}
