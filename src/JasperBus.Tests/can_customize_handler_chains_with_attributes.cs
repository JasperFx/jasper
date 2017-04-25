using System.Linq;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Codegen.StructureMap;
using JasperBus.Model;
using JasperBus.Tests.Runtime;
using Shouldly;
using StructureMap;
using Xunit;

namespace JasperBus.Tests
{

    public class can_customize_handler_chains_with_attributes
    {
        private GenerationConfig theConfig;

        public can_customize_handler_chains_with_attributes()
        {
            theConfig = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            theConfig.Sources.Add(new StructureMapServices(new Container()));
        }

        [Fact]
        public void apply_attribute_on_method()
        {
            var chain = HandlerChain.For<FakeHandler1>(x => x.Handle(null));
            var model = chain.ToGenerationModel(theConfig);

            model.Top.AllFrames().OfType<FakeFrame>().Count().ShouldBe(1);
        }

        [Fact]
        public void apply_attribute_on_class()
        {
            var chain = HandlerChain.For<FakeHandler2>(x => x.Handle(null));
            var model = chain.ToGenerationModel(theConfig);

            model.Top.AllFrames().OfType<FakeFrame>().Count().ShouldBe(1);
        }


    }


    public class FakeHandler1
    {
        [FakeFrame]
        [MaximumAttempts(3)]
        public void Handle(Message1 message)
        {

        }
    }

    [FakeFrame]
    public class FakeHandler2
    {
        public void Handle(Message1 message)
        {

        }
    }

    public class FakeFrameAttribute : ModifyHandlerChainAttribute
    {
        public override void Modify(HandlerChain chain)
        {
            chain.Wrappers.Add(new FakeFrame());
        }
    }

    public class FakeFrame : Frame
    {
        public FakeFrame() : base(false)
        {
        }

        public override void GenerateCode(IGeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// fake frame here");
            Next?.GenerateCode(method, writer);
        }
    }
}