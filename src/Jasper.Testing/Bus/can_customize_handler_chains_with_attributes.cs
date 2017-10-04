using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Internals.Codegen;
using Jasper.Internals.Codegen.ServiceLocation;
using Jasper.Internals.Compilation;
using Jasper.Internals.IoC;
using Jasper.Testing.Bus.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Bus
{

    public class can_customize_handler_chains_with_attributes
    {
        private GenerationRules theRules;

        public can_customize_handler_chains_with_attributes()
        {
            theRules = new GenerationRules("Jasper.Testing.Codegen.Generated");
            theRules.Sources.Add(new ServiceGraph(new ServiceCollection()));
            theRules.Sources.Add(new NoArgConcreteCreator());
        }

        [Fact]
        public void apply_attribute_on_method()
        {
            var chain = HandlerChain.For<FakeHandler1>(x => x.Handle(new Message1()));
            var model = chain.ToClass(theRules);

            model.Methods.Single().Top.AllFrames().OfType<FakeFrame>().Count().ShouldBe(1);
        }

        [Fact]
        public void apply_attribute_on_class()
        {
            var chain = HandlerChain.For<FakeHandler2>(x => x.Handle(null));
            var model = chain.ToClass(theRules);

            model.Methods.Single().Top.AllFrames().OfType<FakeFrame>().Count().ShouldBe(1);
        }

        [Fact]
        public void apply_attribute_on_message_type()
        {
            var chain = HandlerChain.For<FakeHandler1>(x => x.Handle(new ErrorHandledMessage()));
            var model = chain.ToClass(theRules);

            chain.MaximumAttempts.ShouldBe(5);

            model.Methods.Single().Top.AllFrames().OfType<FakeFrame>().Count().ShouldBe(1);
        }
    }


    public class FakeHandler1
    {
        [FakeFrame]
        [MaximumAttempts(3)]
        public void Handle(Message1 message)
        {

        }

        public void Handle(ErrorHandledMessage message)
        {

        }
    }

    [FakeFrame]
    [MaximumAttempts(5)]
    public class ErrorHandledMessage
    {

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
            chain.Middleware.Add(new FakeFrame());
        }
    }

    public class FakeFrame : Frame
    {
        public FakeFrame() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// fake frame here");
            Next?.GenerateCode(method, writer);
        }
    }
}
