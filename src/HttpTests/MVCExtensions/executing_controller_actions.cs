using System.Threading.Tasks;
using Alba;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace HttpTests.MVCExtensions
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
            var container = _app.System.Services.GetRequiredService<IContainer>();
            var what = container.WhatDoIHave(typeof(IStartupFilter));

            _output.WriteLine(what);

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
                x.Get.Url("/hello2");
                x.ContentShouldBe("Hello!");
            });
        }

        [Fact]
        public async Task use_json_writer()
        {
            var response = await _app.System.GetAsJson<Hero>("/json");
            response.Name.ShouldBe("Wolverine");
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
    }

    public class Hero
    {
        public string Name { get; set; }
        public string Affiliation { get; set; }
    }

    public class ExecutingController : ControllerBase
    {
        [HttpGet("hello2")]
        public string Hello()
        {
            return "Hello!";
        }

        // SAMPLE: using-HttpContext-in-Controller
        [HttpGet("write")]
        public Task WriteIntoTheContext()
        {
            return HttpContext.Response.WriteAsync("I wrote some stuff here");
        }
        // ENDSAMPLE

        // SAMPLE: using-IActionResult
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

        // ENDSAMPLE
    }
}
