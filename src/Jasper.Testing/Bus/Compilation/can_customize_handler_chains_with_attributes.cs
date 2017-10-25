using System;
using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Internals.Codegen;
using Jasper.Internals.Codegen.ServiceLocation;
using Jasper.Internals.Compilation;
using Jasper.Internals.IoC;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Bus.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Bus
{

    public class can_customize_handler_chains_with_attributes : IDisposable
    {
        private JasperRuntime _runtime;


        public can_customize_handler_chains_with_attributes()
        {
            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<FakeHandler1>();
                _.Handlers.IncludeType<FakeHandler2>();
            });
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }

        [Fact]
        public void apply_attribute_on_method()
        {
            var chain = _runtime.Get<HandlerGraph>().ChainFor<Message1>();
            chain.SourceCode.ShouldContain("// fake frame here");
        }

        [Fact]
        public void apply_attribute_on_class()
        {
            var chain = _runtime.Get<HandlerGraph>().ChainFor<Message2>();
            chain.SourceCode.ShouldContain("// fake frame here");
        }

        [Fact]
        public void apply_attribute_on_message_type()
        {
            var chain = _runtime.Get<HandlerGraph>().ChainFor<ErrorHandledMessage>();
            chain.SourceCode.ShouldContain("// fake frame here");


            chain.MaximumAttempts.ShouldBe(5);

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
        public void Handle(Message2 message)
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
