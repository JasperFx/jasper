using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using LamarCompiler;
using LamarCompiler.Frames;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public class can_customize_handler_chains_with_attributes
    {
        private void forMessage<T>(Action<HandlerChain> action)
        {
            using (var runtime = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<FakeHandler1>();
                _.Handlers.IncludeType<FakeHandler2>();
            }))
            {
                var chain = runtime.Get<HandlerGraph>().ChainFor<T>();
                action(chain);
            }
        }


        [Fact]
        public void apply_attribute_on_class()
        {
            forMessage<Message2>(chain => chain.SourceCode.ShouldContain("// fake frame here"));
        }

        [Fact]
        public void apply_attribute_on_message_type()
        {
            forMessage<ErrorHandledMessage>(chain =>
            {
                chain.SourceCode.ShouldContain("// fake frame here");
                chain.Retries.MaximumAttempts.ShouldBe(5);
            });
        }

        [Fact]
        public void apply_attribute_on_method()
        {
            forMessage<Message1>(chain => chain.SourceCode.ShouldContain("// fake frame here"));
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
        public override void Modify(HandlerChain chain, JasperGenerationRules rules)
        {
            chain.Middleware.Add(new CustomFrame());
        }
    }

    public class CustomFrame : Frame
    {
        public CustomFrame() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// fake frame here");
            Next?.GenerateCode(method, writer);
        }
    }
}
