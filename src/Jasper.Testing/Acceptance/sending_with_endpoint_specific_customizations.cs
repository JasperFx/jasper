using System.Linq;
using System.Threading.Tasks;
using Jasper.Testing.Compilation;
using Jasper.Testing.Configuration;
using Jasper.Testing.Persistence.Sagas;
using Jasper.Tracking;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class sending_with_endpoint_specific_customizations
    {
        public class DoubleEndpointApp : JasperOptions
        {
            public DoubleEndpointApp()
            {
                Endpoints.PublishAllMessages().ToPort(5678)
                    .CustomizeOutgoing(e => e.Headers.Add("a", "one"))
                    .CustomizeOutgoingMessagesOfType<BaseMessage>(e => e.Headers.Add("d", "four"));

                Endpoints.PublishAllMessages().ToPort(6789)
                    .CustomizeOutgoing(e => e.Headers.Add("b", "two"))
                    .CustomizeOutgoing(e => e.Headers.Add("c", "three"))
                    .CustomizeOutgoingMessagesOfType<SenderConfigurationTests.OtherMessage>(e => e.Headers.Add("e", "five"));

                Endpoints.ListenAtPort(5678);
                Endpoints.ListenAtPort(6789);

                Extensions.UseMessageTrackingTestingSupport();

            }

            public class CustomMessage
            {

            }

            public class DifferentMessage{}

            public class CustomMessageHandler
            {
                public void Handle(CustomMessage message)
                {

                }

                public void Handle(DifferentMessage message)
                {

                }

            }


            [Fact]
            public async Task apply_customizations_to_certain_message_types_for_specific_type()
            {
                using (var host = JasperHost.For<DoubleEndpointApp>())
                {
                    var session = await host.TrackActivity().IncludeExternalTransports()
                        .SendMessageAndWait(new DifferentMessage());

                    var envelopes = session.FindEnvelopesWithMessageType<DifferentMessage>(EventType.Received);

                    var e5678 = envelopes.Single(x => x.Envelope.Destination.Port == 5678).Envelope;
                    e5678.Headers["a"].ShouldBe("one");
                    e5678.Headers.ContainsKey("b").ShouldBeFalse();
                    e5678.Headers.ContainsKey("c").ShouldBeFalse();

                    var e6789 = envelopes.Single(x => x.Envelope.Destination.Port == 6789).Envelope;
                    e6789.Headers.ContainsKey("a").ShouldBeFalse();
                    e6789.Headers["b"].ShouldBe("two");
                    e6789.Headers["c"].ShouldBe("three");
                }
            }
        }



    }
}
