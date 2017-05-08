using System;
using System.Threading.Tasks;
using Baseline;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace JasperHttp.Tests.Compilation
{
    public class using_route_variables : CompilationContext<HandlerWithParameters>
    {


        [Fact]
        public async Task run_string_parameter()
        {
            theContext.SetSegments(new string[] {"go", "Thomas"});
            await Execute(x => x.post_go_name(null, null));


            theContext.Response.Body.Position = 0;
            theContext.Response.Body.ReadAllText().ShouldBe("Thomas");
        }

        [Fact]
        public async Task run_with_parsed_argument_happy_path()
        {
            HandlerWithParameters.Age = 0;

            var segments = new string[] {"go", "43"};

            theContext.SetSegments(segments);

            await Execute(x => x.post_go_age(null, 0));

            HandlerWithParameters.Age.ShouldBe(43);
        }

        [Fact]
        public async Task run_with_parsed_Guid_argument_happy_path()
        {
            HandlerWithParameters.Guid = Guid.Empty;

            var guid = Guid.NewGuid();

            var segments = new string[] { "go", guid.ToString() };

            theContext.SetSegments(segments);

            await Execute(x => x.post_go_guid(null, Guid.Empty));

            HandlerWithParameters.Guid.ShouldBe(guid);
        }

        [Fact]
        public async Task parse_sad_path_for_pure_sync_chain()
        {
            theContext.SetSegments(new string[] {"bad", "garbage"});
            await Execute(x => x.post_bad_guid(Guid.Empty));

            theContext.Response.StatusCode.ShouldBe(400);
        }

        [Fact]
        public async Task parse_sad_path_for_task_returning_methods()
        {
            theContext.SetSegments(new string[] {"badasync", "garbage"});

            await Execute(x => x.post_badasync_guid(Guid.Empty));

            theContext.Response.StatusCode.ShouldBe(400);
        }
    }

    public class HandlerWithParameters
    {
        public Task post_go_name(HttpResponse response, string name)
        {
            return response.WriteAsync(name);
        }

        public Task post_go_age(HttpResponse response, int age)
        {
            Age = age;
            return response.WriteAsync($"Age is {age}");
        }

        public Task post_go_guid(HttpResponse response, Guid guid)
        {
            Guid = guid;
            return response.WriteAsync("Got a guid");
        }

        public void post_bad_guid(Guid guid)
        {
            
        }

        public Task post_badasync_guid(Guid guid)
        {
            return Task.CompletedTask;
        }


        public static Guid Guid { get; set; }
        public static int Age { get; set; }
    }

}
