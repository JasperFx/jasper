using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Testing.Messaging.Bootstrapping;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class default_maximum_attempts : BootstrappingContext
    {
        [Fact]
        public async Task can_set_the_global_default()
        {
            theRegistry.Handlers.DefaultMaximumAttempts = 3;

            (await theHandlers()).ChainFor<SimpleMessage>()
                .MaximumAttempts.ShouldBe(3);
        }

        [Fact]
        public async Task explicit_configuration_always_wins()
        {
            theRegistry.Handlers.DefaultMaximumAttempts = 3;

            (await theHandlers()).ChainFor<Message4>().MaximumAttempts.ShouldBe(11);
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
