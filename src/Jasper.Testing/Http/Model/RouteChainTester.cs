using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Http.Routing.Codegen;
using Jasper.Internals.Codegen;
using Jasper.Internals.Codegen.ServiceLocation;
using Jasper.Internals.Compilation;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Http.Model
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
            ShouldBeNullExtensions.ShouldBeNull(chainFor(x => x.get_command()).InputType);
        }

        [Fact]
        public void determine_resource_type()
        {
            ShouldBeNullExtensions.ShouldBeNull(chainFor(x => x.post_command(null)).ResourceType);
            ShouldBeNullExtensions.ShouldBeNull(chainFor(x => x.post_input()).ResourceType);
            chainFor(x => x.get_resource()).ResourceType.ShouldBe(typeof(Resource1));
            chainFor(x => x.get_resource2()).ResourceType.ShouldBe(typeof(Resource2));
        }

        [Fact]
        public void adds_route_argument_frames_to_the_handle_method_body()
        {
            var chain = chainFor(x => x.post_select_name(null));

            chain.Route.Arguments.Single().ShouldBeOfType<RouteArgument>()
                .Position.ShouldBe(1);

            var generationConfig = new GenerationRules("SomeApp");
            generationConfig.Sources.Add(new NoArgConcreteCreator());

            var @class = chain.ToClass(generationConfig);

            @class.Methods.Single().Frames.OfType<StringRouteArgumentFrame>().Single()
                .Name.ShouldBe("name");


        }


        [Fact]
        public void will_apply_generic_chain_attributes()
        {
            var chain = chainFor(x => x.post_select_name(null));
            var frames = chain.DetermineFrames();

            chain.Middleware.Any(x => x is FakeMiddleware1).ShouldBeTrue();
            chain.Middleware.Any(x => x is FakeMiddleware2).ShouldBeTrue();
        }
    }

    public class FakeMiddleware1 : Frame
    {
        public FakeMiddleware1() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {

        }
    }

    public class FakeMiddleware2 : Frame
    {
        public FakeMiddleware2() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {

        }
    }

    public class RouteChainTarget
    {
        [Middleware(typeof(FakeMiddleware1), typeof(FakeMiddleware2))]
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
            throw new NotSupportedException();
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
