using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Local
{
    public class local_integration_specs : IntegrationContext
    {

        public local_integration_specs(DefaultApp @default) : base(@default)
        {
        }

        private void configure()
        {
            with(_ =>
            {
                _.Endpoints.Publish(x => x.Message<Message1>()
                    .ToLocalQueue("incoming"));

                _.Extensions.UseMessageTrackingTestingSupport();

            });
        }


        [Fact]
        public async Task send_a_message_and_get_the_response()
        {
            configure();

            var message1 = new Message1();
            var session = await Host.SendMessageAndWait(message1);


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .ShouldBeSameAs(message1);
        }
    }
}
