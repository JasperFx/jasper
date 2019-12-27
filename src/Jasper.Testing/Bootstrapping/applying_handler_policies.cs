using System.Linq;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class applying_handler_policies : Runtime.IntegrationContext
    {
        public applying_handler_policies(Runtime.DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void can_apply_a_wrapper_to_all_chains()
        {
            with(_ => _.Handlers.GlobalPolicy<WrapWithSimple>());

            chainFor<MovieAdded>().Middleware.OfType<SimpleWrapper>().Any().ShouldBeTrue();
        }
    }

    public class WrapWithSimple : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
        {
            foreach (var chain in graph.Chains) chain.Middleware.Add(new SimpleWrapper());
        }
    }

    public class SimpleWrapper : Frame
    {
        public SimpleWrapper() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// Just a comment that SimpleWrapper was there");

            Next?.GenerateCode(method, writer);
        }
    }
}
