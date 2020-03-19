using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Compilation
{
    public class can_use_cancellation_token_in_handler_methods : IntegrationContext
    {
        public can_use_cancellation_token_in_handler_methods(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public async Task passes_the_token_into_handler_methods()
        {
            var message = new CancellationTokenUsingMessage();
            await Bus.Invoke(message);

            message.TokenUsed.ShouldNotBeNull();


        }
    }

    public class CancellationTokenUsingMessage
    {
        public CancellationToken TokenUsed { get; set; }
    }

    public class CancellationTokenUsingMessageHandler
    {
        public void Handle(CancellationTokenUsingMessage message, CancellationToken cancellation)
        {
            message.TokenUsed = cancellation;
        }
    }
}
