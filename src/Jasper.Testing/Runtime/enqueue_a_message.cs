using System;
using System.Threading.Tasks;
using Jasper.Testing.Transports.Tcp;
using Jasper.Tracking;
using NSubstitute.Routing.Handlers;
using Shouldly;
using TestingSupport;
using TestingSupport.Compliance;
using TestingSupport.Fakes;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class enqueue_a_message : IntegrationContext
    {
        public enqueue_a_message(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public async Task enqueue_locally()
        {
            var message = new Message1
            {
                Id = Guid.NewGuid()
            };

            var session = await Host.ExecuteAndWait(c => c.Enqueue(message));

            var tracked = session.FindSingleTrackedMessageOfType<Message1>();

            tracked.Id.ShouldBe(message.Id);

        }

    }
}
