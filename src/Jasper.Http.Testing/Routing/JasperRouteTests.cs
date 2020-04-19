using System;
using Jasper.Attributes;
using Jasper.Http.Routing;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Http.Testing.Routing
{
    public class JasperRouteTests
    {
        [Fact]
        public void argument_in_brackets()
        {
            var arg = JasperRoute.ToParameter("{bar}", 3).ShouldBeOfType<RouteArgument>();
            arg.Position.ShouldBe(3);
            arg.Key.ShouldBe("bar");
        }

        [Fact]
        public void argument_starting_with_colon()
        {
            var arg = JasperRoute.ToParameter(":foo", 2).ShouldBeOfType<RouteArgument>();
            arg.Position.ShouldBe(2);
            arg.Key.ShouldBe("foo");
        }

        [Fact]
        public void blank_segment()
        {
            JasperRoute.ToParameter("foo", 0).ShouldBeOfType<Segment>().Path.ShouldBe("foo");
        }

        [Fact]
        public void cannot_have_a_spread()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => { new JasperRoute("GET", "..."); });
        }

        [Fact]
        public void cannot_have_multiple_spreads_either()
        {
            Action action = () => { new JasperRoute("GET", "a/.../b/..."); };

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void cannot_have_only_an_argument()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => { new JasperRoute("GET", ":arg"); });
        }

        [Fact]
        public void spread()
        {
            JasperRoute.ToParameter("...", 4).ShouldBeOfType<Spread>()
                .Position.ShouldBe(4);
        }

        [Fact]
        public void spread_has_to_be_last()
        {
            Action action = () => { new JasperRoute("GET", "a/.../b"); };
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }


        [Fact]
        public void assign_the_handler_type_and_method()
        {
            var route = JasperRoute.Build<SomeEndpoint>(x => x.post_go());
            route.HandlerType.ShouldBe(typeof(SomeEndpoint));
            route.Method.Name.ShouldBe("post_go");
        }

        [Fact]
        public void assign_the_input_type_if_is_one()
        {
            var route = JasperRoute.Build<SomeEndpoint>(x => x.post_something(null));
            route.InputType.ShouldBe(typeof(Input1));
        }

        [Fact]
        public void no_input_type_if_none()
        {
            var route = JasperRoute.Build<SomeEndpoint>(x => x.delete_something(null));
            route.InputType.ShouldBeNull();
        }

        [Fact]
        public void picks_up_custom_route_name_from_attribute_if_any()
        {
            var route = JasperRoute.Build<SomeEndpoint>(x => x.get_named());
            route.Name.ShouldBe("Finn");
        }

        [Fact]
        public void support_the_one_in_model()
        {
            var route = JasperRoute.Build<SomeEndpoint>(x => x.put_message1(null));
            route.InputType.ShouldBe(typeof(Message1));
        }

        [Fact]
        public void use_dash_in_route()
        {
            JasperRoute.Build<DashAndUnderscoreEndpoint>(x => x.get_cool___stuff())
                .Pattern.ShouldBe("cool-stuff");
        }

        [Fact]
        public void use_underscore_in_route()
        {
            JasperRoute.Build<DashAndUnderscoreEndpoint>(x => x.get__text())
                .Pattern.ShouldBe("_text");
        }
    }

    public class building_a_route_from_segments_Tests
    {
        public building_a_route_from_segments_Tests()
        {
            segments = new ISegment[]
                {new Segment("folder", 0), new Segment("folder2", 1), new RouteArgument("name", 2)};


            route = new JasperRoute(segments, HttpVerbs.PUT);
        }

        private readonly ISegment[] segments;
        private readonly JasperRoute route;

        [Fact]
        public void should_build_the_pattern_from_the_segments()
        {
            route.Pattern.ShouldBe("folder/folder2/:name");
        }

        [Fact]
        public void should_remember_the_httpMethod()
        {
            route.HttpMethod.ShouldBe(HttpVerbs.PUT);
        }

        [Fact]
        public void still_derives_the_name()
        {
            route.Name.ShouldBe("PUT:folder/folder2/:name");
        }

        [Fact]
        public void still_has_the_original_segments()
        {
            route.Segments.ShouldHaveTheSameElementsAs(segments);
        }




    }

    [JasperIgnore]
    public class SomeEndpoint
    {
        public void post_go()
        {
        }

        public void post_something(Input1 input)
        {
        }

        public void delete_something(string name)
        {
        }

        [RouteName("Finn")]
        public void get_named()
        {
            throw new NotImplementedException();
        }

        public void put_message1(Message1 something)
        {
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
