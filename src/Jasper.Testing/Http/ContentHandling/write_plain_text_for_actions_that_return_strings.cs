using System.Runtime.Loader;
using Jasper.Http;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public static class HttpTestingApp
    {
        public static string RootUrl = "http://localhost:5666";

        private static readonly JasperRuntime _runtime;

        static HttpTestingApp()
        {
            var registry = new JasperRegistry();
            registry.UseFeature<AspNetCoreFeature>();
            registry.Feature<AspNetCoreFeature>().Hosting.Port = 5666;

            _runtime = JasperRuntime.For(registry);

            AssemblyLoadContext.Default.Unloading += context =>
            {
                _runtime.Dispose();
            };
        }
    }


    public class write_plain_text_for_actions_that_return_strings
    {
        [Fact]
        public void write_as_text()
        {

        }
    }
}
