using System.Threading.Tasks;
using Alba;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.MvcExtender.Tests
{
    public class executing_controller_actions : IClassFixture<MvcExtendedApp>
    {
        private readonly MvcExtendedApp _app;
        private readonly ITestOutputHelper _output;

        public executing_controller_actions(MvcExtendedApp app, ITestOutputHelper output)
        {
            _app = app;
            _output = output;
        }

        [Fact]
        public Task run_simple_method_that_returns_string()
        {
            return _app.System.Scenario(x =>
            {
                x.Get.Url("/hello");
                x.ContentShouldBe("Hello!");
            });
        }


    }

    public class ExecutingController : ControllerBase
    {
        [HttpGet("hello")]
        public string Hello()
        {
            return "Hello!";
        }
    }
}
