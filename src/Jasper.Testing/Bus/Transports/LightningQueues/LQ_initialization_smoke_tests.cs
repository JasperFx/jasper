using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.LightningQueues;
using Jasper.Testing.Bus.Runtime;
using Xunit;

namespace Jasper.Testing.Bus.Transports.LightningQueues
{
    public class LQ_initialization_smoke_tests : IntegrationContext
    {
        public LQ_initialization_smoke_tests()
        {
            LightningQueuesTransport.DeleteAllStorage();
        }

        [Fact]
        public void start_up_listening_on_a_channel()
        {
            with(_ =>
            {
                _.ListenForMessagesFrom("lq.tcp://localhost:2200/incoming".ToUri());
            });
        }

        [Fact]
        public void start_up_listening_with_only_senders()
        {
            with(_ =>
            {
                _.SendMessagesFromAssemblyContaining<Message1>()
                    .To("lq.tcp://localhost:2200/outgoing".ToUri());
            });
        }
    }
}