using System.Linq;
using Baseline;
using Jasper.Bus.Model;
using Jasper.Testing.Bus.Bootstrapping;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.ErrorHandling
{
    public class default_maximum_attempts : BootstrappingContext
    {
        [Fact]
        public void can_set_the_global_default()
        {
            theRegistry.Handlers.DefaultMaximumAttempts = 3;

            theHandlers.ChainFor<SimpleMessage>()
                .MaximumAttempts.ShouldBe(3);
        }

        [Fact]
        public void explicit_configuration_always_wins()
        {
            theRegistry.Handlers.DefaultMaximumAttempts = 3;

            theHandlers.ChainFor<Message4>().MaximumAttempts.ShouldBe(11);
        }
    }

    public class SimpleMessage
    {

    }

    public class NotOverriddenHandler
    {
        public void Handle(SimpleMessage message)
        {

        }
    }

    public class OverriddenAttemptsHandler
    {
        public static void Configure(HandlerChain chain)
        {
            chain.MaximumAttempts = 11;
        }

        public void Handle(Message4 message)
        {

        }
    }
}
