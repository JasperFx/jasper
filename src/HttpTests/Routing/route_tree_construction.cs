using System.Linq;
using Jasper.Configuration;
using JasperHttp;
using JasperHttp.Routing;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class route_tree_construction
    {
        private readonly RouteTree theTree =
            new RouteTree(new JasperHttpOptions(), new JasperGenerationRules("JasperGenerated"));

        [Fact]
        public void arg_at_the_end_one_deep()
        {
            var route = Route.For("planets/:planet", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("planets").ArgRoutes.ShouldContain(route);
        }

        [Fact]
        public void arg_at_the_end_two_deep()
        {
            var route = Route.For("world/planets/:planet", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("world").ChildFor("planets").ArgRoutes.ShouldContain(route);
        }

        [Fact]
        public void complex_arg_route()
        {
            var route = Route.For("world/from/:start/to/:end", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("world").ChildFor("from").ArgRoutes.ShouldContain(route);
        }

        [Fact]
        public void just_off_root()
        {
            var route = Route.For("planets", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").Leaves.Single().ShouldBeSameAs(route);
        }

        [Fact]
        public void place_root_by_method()
        {
            var route = Route.For("/", "PUT");
            route.Place(theTree);

            theTree.ForMethod("PUT").Root.ShouldBeSameAs(route);
        }

        [Fact]
        public void spread_at_second_level()
        {
            var route = Route.For("planets/...", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("planets").SpreadRoute.ShouldBeSameAs(route);
        }

        [Fact]
        public void spread_at_third_level()
        {
            var route = Route.For("planets/hoth/...", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("planets").ChildFor("hoth").SpreadRoute.ShouldBeSameAs(route);
        }

        [Fact]
        public void three_deep()
        {
            var route = Route.For("planets/hoth/snow", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("planets").ChildFor("hoth").Leaves.Single().ShouldBeSameAs(route);
        }

        [Fact]
        public void two_deep()
        {
            var route = Route.For("planets/hoth", "GET");

            route.Place(theTree);

            theTree.ForMethod("GET").ChildFor("planets").Leaves.Single().ShouldBeSameAs(route);
        }
    }
}
