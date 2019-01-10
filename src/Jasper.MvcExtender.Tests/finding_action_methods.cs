using System;
using System.Linq;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.MvcExtender.Tests
{
    public class MvcExtendedApp : IDisposable
    {
        public MvcExtendedApp()
        {
            Host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseJasper()
                .UseServer(new NulloServer())
                .Build();


            Routes = Host.Services.GetRequiredService<RouteGraph>();
        }

        public RouteGraph Routes { get; set; }


        public IWebHost Host { get; }

        public void Dispose()
        {
            Host?.Dispose();
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }

    public class finding_action_methods : IClassFixture<MvcExtendedApp>
    {
        private readonly MvcExtendedApp _app;

        public finding_action_methods(MvcExtendedApp app)
        {
            _app = app;
        }

        [Fact]
        public void will_find_jasper_actions_on_controller_base()
        {
            var chain = _app.Routes.ChainForAction<MyController1>(x => x.get_stuff());
            chain.ShouldNotBeNull();
            chain.Route.Pattern.ShouldBe("stuff");
        }

        [Fact]
        public void will_find_jasper_actions_on_controller()
        {
            var chain = _app.Routes.ChainForAction<MyController2>(x => x.get_stuff_other());
            chain.ShouldNotBeNull();
            chain.Route.Pattern.ShouldBe("stuff/other");
        }

        [Fact]
        public void can_find_and_determine_route_from_HttpGet_marked_method_with_not_arguments()
        {
            var chain = _app.Routes.ChainForAction<MyController1>(x => x.Get1());
            chain.ShouldNotBeNull();
            chain.Route.Pattern.ShouldBe("one");
        }

        [Fact]
        public void can_find_and_determine_route_from_HttpGet_marked_method_with_one_argument()
        {
            var chain = _app.Routes.ChainForAction<MyController1>(x => x.GetDog("Shiner"));
            chain.ShouldNotBeNull();
            chain.Route.Pattern.ShouldBe("dog/:name");
            chain.Route.Segments.ElementAt(1).ShouldBeOfType<RouteArgument>()
                .MappedParameter.Name.ShouldBe("name");
        }
    }

    public class MyController1 : ControllerBase
    {
        public string get_stuff()
        {
            return "stuff";
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
    }

    public class MyController2 : Controller
    {
        public string get_stuff_other()
        {
            return "other stuff";
        }
    }
}
