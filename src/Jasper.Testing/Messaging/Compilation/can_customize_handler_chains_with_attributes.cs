using System;
using System.Threading.Tasks;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Testing.Messaging.Runtime;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Compilation;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public class can_customize_handler_chains_with_attributes
    {


        private async Task forMessage<T>(Action<HandlerChain> action)
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<FakeHandler1>();
                _.Handlers.IncludeType<FakeHandler2>();
            });

            var chain = runtime.Get<HandlerGraph>().ChainFor<T>();

            try
            {
                action(chain);
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


        [Fact]
        public Task apply_attribute_on_class()
        {
            return forMessage<Message2>(chain => chain.SourceCode.ShouldContain("// fake frame here"));
        }

        [Fact]
        public Task apply_attribute_on_message_type()
        {
            return forMessage<ErrorHandledMessage>(chain =>
            {
                chain.SourceCode.ShouldContain("// fake frame here");
                chain.MaximumAttempts.ShouldBe(5);
            });


        }

        [Fact]
        public Task apply_attribute_on_method()
        {
            return forMessage<Message1>(chain => chain.SourceCode.ShouldContain("// fake frame here"));
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
