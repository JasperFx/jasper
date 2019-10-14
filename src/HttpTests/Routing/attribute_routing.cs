using System;
using System.Linq;
using Alba;
using JasperHttp.Model;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class SampleAppWithRoutedAttributes : IDisposable
    {
        public SampleAppWithRoutedAttributes()
        {
            System = SystemUnderTest.For(x => x.UseStartup<Startup>().UseJasper(_ =>
            {
                _.Http(opts =>
                {
                    opts.DisableConventionalDiscovery()
                        .IncludeType<AttributeUsingEndpointClass>()
                        .IncludeType<IdiomaticJasperRouteEndpoint>();
                });
            }));


            Routes = System.Services.GetRequiredService<RouteGraph>();
        }

        public RouteGraph Routes { get; set; }


        public SystemUnderTest System { get; }

        public void Dispose()
        {
            System?.Dispose();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }


    public class finding_action_methods : IClassFixture<SampleAppWithRoutedAttributes>
    {
        public finding_action_methods(SampleAppWithRoutedAttributes app)
        {
            _app = app;
        }

        private readonly SampleAppWithRoutedAttributes _app;

        [Fact]
        public void can_find_and_determine_route_from_HttpPost_marked_method_with_no_arguments()
        {
            var chain = _app.Routes.ChainForAction<AttributeUsingEndpointClass>(x => x.Post1());

            chain.ShouldNotBeNull();
            chain.Route.HttpMethod.ShouldBe("POST");
            chain.Route.Pattern.ShouldBe("one");
        }

        [Fact]
        public void can_find_and_determine_route_from_JasperGet_marked_method_with_no_arguments()
        {
            var chain = _app.Routes.ChainForAction<AttributeUsingEndpointClass>(x => x.Get1());
            chain.ShouldNotBeNull();
            chain.Route.HttpMethod.ShouldBe("GET");
            chain.Route.Pattern.ShouldBe("one");
        }

        [Fact]
        public void can_find_and_determine_route_from_JasperGet_marked_method_with_one_argument()
        {
            var chain = _app.Routes.ChainForAction<AttributeUsingEndpointClass>(x => x.GetDog("Shiner"));
            chain.ShouldNotBeNull();
            chain.Route.Pattern.ShouldBe("dog/:name");
            chain.Route.HttpMethod.ShouldBe("GET");
            chain.Route.Segments.ElementAt(1).ShouldBeOfType<RouteArgument>()
                .MappedParameter.Name.ShouldBe("name");
        }

        [Fact]
        public void will_find_jasper_actions_on_controller()
        {
            var chain = _app.Routes.ChainForAction<IdiomaticJasperRouteEndpoint>(x => x.get_stuff_other());
            chain.ShouldNotBeNull();
            chain.Route.HttpMethod.ShouldBe("GET");
            chain.Route.Pattern.ShouldBe("stuff/other");
        }

        [Fact]
        public void will_find_jasper_actions_on_controller_base()
        {
            var chain = _app.Routes.ChainForAction<AttributeUsingEndpointClass>(x => x.get_stuff());
            chain.ShouldNotBeNull();
            chain.Route.HttpMethod.ShouldBe("GET");
            chain.Route.Pattern.ShouldBe("stuff");
        }
    }


    public class AttributeUsingEndpointClass
    {
        public string get_stuff()
        {
            return "stuff";
        }

        // SAMPLE: AttributeUsingEndpoint
        [HttpPost("one")]
        public int Post1()
        {
            return 200;
        }

        [HttpGet("/one")]
        public string Get1()
        {
            return "one";
        }

        [HttpGet("/dog/{name}")]
        public string GetDog(string name)
        {
            return $"the dog is {name}";
        }

        // ENDSAMPLE
    }


    // SAMPLE: ControllerUsingJasperRouting
    public class IdiomaticJasperRouteEndpoint
    {
        // Use idiomatic Jasper routing
        // This would respond to "GET: /stuff/other"
        public string get_stuff_other()
        {
            return "other stuff";
        }
    }

    // ENDSAMPLE
}
