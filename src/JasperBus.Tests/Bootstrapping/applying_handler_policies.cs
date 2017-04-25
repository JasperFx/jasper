using System.Linq;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using JasperBus.Model;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    public class applying_handler_policies : IntegrationContext
    {
        [Fact]
        public void can_apply_a_wrapper_to_all_chains()
        {
            with(_ => _.Policies.Global<WrapWithSimple>());

            chainFor<MovieAdded>().Wrappers.OfType<SimpleWrapper>().Any()
                .ShouldBeTrue();
        }
    }

    public class WrapWithSimple : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph)
        {
            foreach (var chain in graph.Chains)
            {
                chain.Wrappers.Add(new SimpleWrapper());
            }
        }
    }

    public class SimpleWrapper : Frame
    {
        public SimpleWrapper() : base(false)
        {
        }

        public override void GenerateCode(IGeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// Just a comment that SimpleWrapper was there");

            Next?.GenerateCode(method, writer);
        }
    }
}