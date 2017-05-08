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
    }

    public class HandlerWithParameters
    {
        public Task post_go_name(HttpResponse response, string name)
        {
            return response.WriteAsync(name);
        }
    }


}
