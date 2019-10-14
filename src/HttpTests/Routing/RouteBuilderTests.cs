using System;
using Jasper.Configuration;
using JasperHttp.Routing;
using Shouldly;
using TestMessages;
using Xunit;

namespace HttpTests.Routing
{
    public class RouteBuilderTests
    {
        [Fact]
        public void assign_the_handler_type_and_method()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.post_go());
            route.HandlerType.ShouldBe(typeof(SomeEndpoint));
            route.Method.Name.ShouldBe("post_go");
        }

        [Fact]
        public void assign_the_input_type_if_is_one()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.post_something(null));
            route.InputType.ShouldBe(typeof(Input1));
        }

        [Fact]
        public void no_input_type_if_none()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.delete_something(null));
            route.InputType.ShouldBeNull();
        }

        [Fact]
        public void picks_up_custom_route_name_from_attribute_if_any()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.get_named());
            route.Name.ShouldBe("Finn");
        }

        [Fact]
        public void support_the_one_in_model()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.put_message1(null));
            route.InputType.ShouldBe(typeof(Message1));
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
}
