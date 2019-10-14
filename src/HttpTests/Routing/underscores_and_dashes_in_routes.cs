using JasperHttp.Routing;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class underscores_and_dashes_in_routes
    {
        [Fact]
        public void use_dash_in_route()
        {
            RouteBuilder.Build<DashAndUnderscoreEndpoint>(x => x.get_cool___stuff())
                .Pattern.ShouldBe("cool-stuff");
        }

        [Fact]
        public void use_underscore_in_route()
        {
            RouteBuilder.Build<DashAndUnderscoreEndpoint>(x => x.get__text())
                .Pattern.ShouldBe("_text");
        }
    }

    public class DashAndUnderscoreEndpoint
    {
        // SAMPLE: using-dash-and-underscore-in-routes
        // Responds to "GET: /_text"
        public string get__text()
        {
            return "some text";
        }

        // Responds to "GET: /cool-stuff"
        public string get_cool___stuff()
        {
            return "some cool stuff";
        }

        // ENDSAMPLE
    }
}
