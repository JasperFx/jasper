using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Alba;
using AlbaForJasper;
using Jasper.Http;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
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

    public class HttpTestingApp : JasperRegistry
    {
        public HttpTestingApp()
        {
            Handlers.ConventionalDiscoveryDisabled = true;
        }
    }


    public class write_plain_text_for_actions_that_return_strings
    {
        //[Fact]
        public Task write_as_text()
        {
            return HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/string");
                _.ContentShouldBe("some string");
                _.ContentTypeShouldBe("text/plain");
            });
        }
    }

    public class StringEndpoint
    {
        public string get_string()
        {
            return "some string";
        }
    }
}
