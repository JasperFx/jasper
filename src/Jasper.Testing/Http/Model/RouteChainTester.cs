using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using JasperHttp.Model;
using JasperHttp.Routing;
using JasperHttp.Routing.Codegen;
using Shouldly;
using StructureMap;
using Xunit;

namespace JasperHttp.Tests.Model
{
    public class RouteChainTester
    {
        private RouteChain chainFor(Expression<Action<RouteChainTarget>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            return new RouteChain(new MethodCall(typeof(RouteChainTarget), method));
        }

        [Fact]
        public void can_determine_the_http_method_with_default_conventions()
        {
            chainFor(x => x.get_command()).Route.HttpMethod.ShouldBe("GET");
            chainFor(x => x.put_command()).Route.HttpMethod.ShouldBe("PUT");
            chainFor(x => x.post_command(null)).Route.HttpMethod.ShouldBe("POST");
        }

        [Fact]
        public void determine_input_type_if_there_is_one()
        {
            chainFor(x => x.post_command(null)).InputType.ShouldBe(typeof(Input1));
            chainFor(x => x.get_command()).InputType.ShouldBeNull();
        }

        [Fact]
        public void determine_resource_type()
        {
            chainFor(x => x.post_command(null)).ResourceType.ShouldBeNull();
            chainFor(x => x.post_input()).ResourceType.ShouldBeNull();
            chainFor(x => x.get_resource()).ResourceType.ShouldBe(typeof(Resource1));
            chainFor(x => x.get_resource2()).ResourceType.ShouldBe(typeof(Resource2));
        }

        [Fact]
        public void adds_route_argument_frames_to_the_handle_method_body()
        {
            var chain = chainFor(x => x.post_select_name(null));

            chain.Route.Arguments.Single().ShouldBeOfType<RouteArgument>()
                .Position.ShouldBe(1);

            var generationConfig = new GenerationConfig("SomeApp");
            generationConfig.Sources.Add(new StructureMapServices(new Container()));

            var @class = chain.ToClass(generationConfig);

            @class.Methods.Single().Frames.OfType<StringRouteArgument>().Single()
                .Name.ShouldBe("name");


        }
    }

    public class RouteChainTarget
    {
        public void post_select_name(string name)
        {

        }

        public void post_command(Input1 input)
        {

        }

        public string get_command()
        {
            return string.Empty;
        }


        public void put_command()
        {

        }

        public Task post_input()
        {
            return Task.CompletedTask;
        }

        public Resource1 get_resource()
        {
            return new Resource1();
        }

        public Task<Resource2> get_resource2()
        {
            throw new NotImplementedException();
        }
    }

    public class Input1
    {

    }

    public class Resource1
    {

    }

    public class Resource2
    {

    }

}
