using Jasper.Http.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Routing
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
        public string Index()
        {
            return "Hello, world";
        }

        public string Get()
        {
            return "Hello, world";
        }

        public string Put()
        {
            return "Hello, world";
        }

        public string Delete()
        {
            return "Hello, world";
        }
    }


    [JasperIgnore]
    public class ServiceEndpoint
    {
        public string Index()
        {
            return "Hello, world";
        }

        public string Get()
        {
            return "Hello, world";
        }

        public string Put()
        {
            return "Hello, world";
        }

        public string Delete()
        {
            return "Hello, world";
        }
    }
}
