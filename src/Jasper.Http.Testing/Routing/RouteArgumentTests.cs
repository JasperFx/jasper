using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.Reflection;
using Jasper.Http.Routing;
using Jasper.Http.Routing.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Routing
{
    public class RouteArgumentTests
    {
        public class SomeEndpoint
        {
            public void go(string name, int number)
            {
            }
        }


        [Fact]
        public void can_override_the_arg_type()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType = typeof(int);

            parameter.ArgType.ShouldBe(typeof(int));
        }


        [Fact]
        public void create_route_parsing_frame_from_string_argument()
        {
            var arg = new RouteArgument("name", 1);
            arg.ArgType = typeof(string);

            var frame = arg.ToParsingFrame(null).ShouldBeOfType<CastRouteArgumentFrame>();
            frame.ShouldNotBeNull();
            frame.Position.ShouldBe(1);
            frame.Name.ShouldBe("name");
        }

        [Fact]
        public void create_route_parsing_from_int_argument()
        {
            var arg = new RouteArgument("age", 3) {ArgType = typeof(int)};

            var frame = arg.ToParsingFrame(null).ShouldBeOfType<ParsedRouteArgumentFrame>();
            frame.ShouldNotBeNull();
            frame.Position.ShouldBe(3);
            frame.Variable.Usage.ShouldBe("age");

            frame.Variable.VariableType.ShouldBe(typeof(int));
        }


        [Fact]
        public void read_route_data_from_arguments()
        {
            var method = ReflectionHelper.GetMethod<SomeEndpoint>(x => x.go(null, 3));
            var param = method.GetParameters().Last();

            var arg = new RouteArgument("number", 0);
            arg.MappedParameter = param;

            var arguments = MethodCallParser.ToArguments<SomeEndpoint>(x => x.go(null, 3));

            arg.ReadRouteDataFromMethodArguments(arguments).ShouldBe("3");
        }


        [Fact]
        public void set_mapped_parameter()
        {
            var method = ReflectionHelper.GetMethod<SomeEndpoint>(x => x.go(null, 3));
            var param = method.GetParameters().Last();

            var arg = new RouteArgument("number", 0);
            arg.MappedParameter = param;

            arg.ArgType.ShouldBe(typeof(int));
            arg.MappedParameter.ShouldBe(param);
        }

        [Fact]
        public void the_default_arg_type_is_string()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType.ShouldBe(typeof(string));
        }

    }
}
