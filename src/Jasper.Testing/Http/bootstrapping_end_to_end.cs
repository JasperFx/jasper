using System.Linq;
using System.Threading.Tasks;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class bootstrapping_end_to_end
    {
        private readonly JasperRuntime theRuntime;

        public bootstrapping_end_to_end()
        {
            var registry = new JasperRegistry();
            registry.UseFeature<HttpFeature>();
            registry.Services.AddService<IFakeStore, FakeStore>();

            theRuntime = JasperRuntime.For(registry);
        }

        [Fact]
        public void route_graph_is_registered()
        {
            ShouldBeBooleanExtensions.ShouldBeTrue(theRuntime.Container.GetInstance<RouteGraph>()
                                              .Any());
        }

        [Fact]
        public void can_find_and_apply_endpoints_by_type_scanning()
        {
            var routes = theRuntime.Container.GetInstance<RouteGraph>();

            ShouldBeBooleanExtensions.ShouldBeTrue(routes.Any(x => x.Action.HandlerType == typeof(SimpleEndpoint)));
            ShouldBeBooleanExtensions.ShouldBeTrue(routes.Any(x => x.Action.HandlerType == typeof(OtherEndpoint)));
        }

        [Fact]
        public void url_registry_is_registered()
        {
            ShouldBeBooleanExtensions.ShouldBeTrue(theRuntime.Container.Model.HasDefaultImplementationFor<IUrlRegistry>());
        }

        [Fact]
        public void reverse_url_lookup_by_method()
        {
            var urls = theRuntime.Container.GetInstance<IUrlRegistry>();
            urls.UrlFor<SimpleEndpoint>(x => x.get_hello()).ShouldBe("/hello");
        }

        [Fact]
        public void reverse_url_lookup_by_input_model()
        {
            var urls = theRuntime.Container.GetInstance<IUrlRegistry>();
            urls.UrlFor<OtherModel>().ShouldBe("/other");
        }
    }

    public class SimpleEndpoint
    {
        public string get_hello()
        {
            return "hello";
        }

        public static Task put_simple(HttpResponse response)
        {
            response.ContentType = "text/plain";
            return response.WriteAsync("Simple is as simple does");
        }
    }

    public class OtherEndpoint
    {
        public void delete_other(HttpContext context)
        {
            context.Items.Add("delete_other", "called");
        }

        public void put_other(OtherModel input)
        {
            
        }
    }

    public class OtherModel { }
}