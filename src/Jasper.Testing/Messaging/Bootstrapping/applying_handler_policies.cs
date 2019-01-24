using System.Linq;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using LamarCompiler;
using LamarCompiler.Frames;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class applying_handler_policies : IntegrationContext
    {
        [Fact]
        public void can_apply_a_wrapper_to_all_chains()
        {
            with(_ => _.Handlers.GlobalPolicy<WrapWithSimple>());

            chainFor<MovieAdded>().Middleware.OfType<SimpleWrapper>().Any().ShouldBeTrue();
        }
    }

    public class WrapWithSimple : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, JasperGenerationRules rules)
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
