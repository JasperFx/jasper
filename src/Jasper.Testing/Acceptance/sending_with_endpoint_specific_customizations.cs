using System.Linq;
using System.Threading.Tasks;
using Jasper.Testing.Compilation;
using Jasper.Testing.Configuration;
using Jasper.Testing.Persistence.Sagas;
using Jasper.Tracking;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class sending_with_endpoint_specific_customizations
    {
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
            using var host = JasperHost.For(opts =>
            {
                opts.PublishAllMessages().To("stub://one")
                    .CustomizeOutgoing(e => e.Headers.Add("a", "one"))
                    .CustomizeOutgoingMessagesOfType<BaseMessage>(e => e.Headers.Add("d", "four"));

                opts.PublishAllMessages().To("stub://two")
                    .CustomizeOutgoing(e => e.Headers.Add("b", "two"))
                    .CustomizeOutgoing(e => e.Headers.Add("c", "three"))
                    .CustomizeOutgoingMessagesOfType<SenderConfigurationTests.OtherMessage>(e => e.Headers.Add("e", "five"));

                opts.ListenForMessagesFrom("stub://5678");
                opts.ListenForMessagesFrom("stub://6789");

                opts.Extensions.UseMessageTrackingTestingSupport();
            });
            var session = await host.TrackActivity().IncludeExternalTransports()
                .SendMessageAndWait(new DifferentMessage());

            var envelopes = session.FindEnvelopesWithMessageType<DifferentMessage>(EventType.Sent);

            var env1 = envelopes.Single(x => x.Envelope.Destination == "stub://one".ToUri()).Envelope;
            env1.Headers["a"].ShouldBe("one");
            env1.Headers.ContainsKey("b").ShouldBeFalse();
            env1.Headers.ContainsKey("c").ShouldBeFalse();

            var env2 = envelopes.Single(x => x.Envelope.Destination == "stub://two".ToUri()).Envelope;
            env2.Headers.ContainsKey("a").ShouldBeFalse();
            env2.Headers["b"].ShouldBe("two");
            env2.Headers["c"].ShouldBe("three");
        }


    }
}
