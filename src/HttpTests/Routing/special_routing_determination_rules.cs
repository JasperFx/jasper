using Jasper.Configuration;
using JasperHttp.Routing;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class special_routing_determination_rules
    {
        [Fact]
        public void homeendpoint()
        {
            RouteBuilder.Build<HomeEndpoint>(x => x.Index()).MethodAndPatternShouldBe("GET", "");
            RouteBuilder.Build<HomeEndpoint>(x => x.Get()).MethodAndPatternShouldBe("GET", "");
            RouteBuilder.Build<HomeEndpoint>(x => x.Put()).MethodAndPatternShouldBe("PUT", "");
            RouteBuilder.Build<HomeEndpoint>(x => x.Delete()).MethodAndPatternShouldBe("DELETE", "");
        }

        [Fact]
        public void serviceendpoint()
        {
            RouteBuilder.Build<ServiceEndpoint>(x => x.Index()).MethodAndPatternShouldBe("GET", "");
            RouteBuilder.Build<ServiceEndpoint>(x => x.Get()).MethodAndPatternShouldBe("GET", "");
            RouteBuilder.Build<ServiceEndpoint>(x => x.Put()).MethodAndPatternShouldBe("PUT", "");
            RouteBuilder.Build<ServiceEndpoint>(x => x.Delete()).MethodAndPatternShouldBe("DELETE", "");
        }
    }

    public static class RouteSpecificationExtensions
    {
        public static void MethodAndPatternShouldBe(this Route route, string method, string pattern)
        {
            route.HttpMethod.ShouldBe(method);
            route.Pattern.ShouldBe(pattern);
        }
    }

    [JasperIgnore]
    public class HomeEndpoint
    {
        // Responds to GET: /
        public string Index()
        {
            return "Hello, world";
        }

        // Responds to GET: /
        public string Get()
        {
            return "Hello, world";
        }

        // Responds to PUT: /
        public void Put()
        {
        }

        // Responds to DELETE: /
        public void Delete()
        {
        }
    }


    [JasperIgnore]
    // SAMPLE: ServiceEndpoint
    public class ServiceEndpoint
    {
        // GET: /
        public string Index()
        {
            return "Hello, world";
        }

        // GET: /
        public string Get()
        {
            return "Hello, world";
        }

        // PUT: /
        public string Put()
        {
            return "Hello, world";
        }

        // DELETE: /
        public string Delete()
        {
            return "Hello, world";
        }
    }

    // ENDSAMPLE
}
