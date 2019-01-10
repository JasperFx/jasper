using System.Threading.Tasks;
using Alba;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.MvcExtender.Tests
{
    public class executing_controller_actions : IClassFixture<MvcExtendedApp>
    {
        public executing_controller_actions(MvcExtendedApp app, ITestOutputHelper output)
        {
            _app = app;
            _output = output;
        }

        private readonly MvcExtendedApp _app;
        private readonly ITestOutputHelper _output;

        [Fact]
        public Task run_controller_action_that_uses_http_context_object()
        {
            return _app.System.Scenario(x =>
            {
                x.Get.Url("/write");
                x.ContentShouldContain("I wrote some stuff here");
            });
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

        [Fact]
        public Task use_simplistic_action_result()
        {
            return _app.System.Scenario(x =>
            {
                x.Get.Url("/result");
                x.StatusCodeShouldBe(202);
            });
        }

        [Fact]
        public async Task use_json_writer()
        {
            var response = await _app.System.GetAsJson<Hero>("/json");
            response.Name.ShouldBe("Wolverine");
        }
    }

    public class Hero
    {
        public string Name { get; set; }
        public string Affiliation { get; set; }
    }

    public class ExecutingController : ControllerBase
    {
        [HttpGet("hello")]
        public string Hello()
        {
            return "Hello!";
        }

        [HttpGet("write")]
        public Task WriteIntoTheContext()
        {
            return HttpContext.Response.WriteAsync("I wrote some stuff here");
        }

        [HttpGet("result")]
        public IActionResult Result()
        {
            return StatusCode(202);
        }

        [HttpGet("json")]
        public JsonResult WriteJson()
        {
            return new JsonResult(new Hero
            {
                Name = "Wolverine",
                Affiliation = "Xmen"
            });
        }
    }
}
