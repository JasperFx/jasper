using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Tracking;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Transports.Local
{
    public class local_integration_specs : IntegrationContext
    {

        public local_integration_specs(DefaultApp @default) : base(@default)
        {
        }

        private void configure()
        {
            with(opts =>
            {
                opts.Publish(x => x.Message<Message1>()
                    .ToLocalQueue("incoming"));

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseSystemTextJsonForSerialization();

            });
        }


        [Fact]
        public async Task send_a_message_and_get_the_response()
        {
            configure();

            var message1 = new Message1();
            var session = await Host.SendMessageAndWaitAsync(message1, 15000);


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .ShouldBeSameAs(message1);
        }
    }
}
