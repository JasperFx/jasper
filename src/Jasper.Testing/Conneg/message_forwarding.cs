using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Transports;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class message_forwarding
    {
        [Fact]
        public void pick_up_message_alias_from_upstream()
        {
            typeof(OriginalMessage).ToMessageAlias().ShouldBe("versioned-message");
            typeof(NewMessage).ToMessageAlias().ShouldBe("versioned-message");
        }

        [Fact]
        public void automatically_discover_forwarding_types_for_known_incoming_messages()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Handlers.IncludeType<NewMessageHandler>();

                _.Http.Actions.ExcludeTypes(t => true);
            }))
            {
                var modelReader = runtime.Get<BusMessageSerializationGraph>().ReaderFor(typeof(NewMessage));

                var reader = modelReader.OfType<ForwardingMessageDeserializer<NewMessage>>().Single();
                reader.ShouldNotBeNull();

                modelReader.ContentTypes.OrderBy(x => x).ShouldHaveTheSameElementsAs("application/json", typeof(OriginalMessage).ToContentType("json"), typeof(NewMessage).ToContentType("json"));
            }
        }

        [Fact]
        public async Task send_message_via_forwarding()
        {
            var tracker = new MessageTracker();
            var channel = "tcp://localhost:2345/incoming".ToUri();

            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Handlers.IncludeType<NewMessageHandler>();

                _.Services.AddSingleton(tracker);


                _.Publish.Message<OriginalMessage>().To(channel);
                _.Transports.ListenForMessagesFrom(channel);
            }))
            {
                var waiter = tracker.WaitFor<NewMessage>();

                await runtime.Bus.Send(new OriginalMessage {FirstName = "James", LastName = "Worthy"}, e =>
                {
                    e.Destination = channel;
                    e.ContentType = typeof(OriginalMessage).ToContentType("json");
                });

                waiter.Wait(5.Seconds());

                waiter.Result.Message.As<NewMessage>().FullName.ShouldBe("James Worthy");
            }
        }
    }

    [Version("V1")]
    public class OriginalMessage : IForwardsTo<NewMessage>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public NewMessage Transform()
        {
            return new NewMessage{FullName = $"{FirstName} {LastName}"};
        }
    }

    [Version("V2"), MessageAlias("versioned-message")]
    public class NewMessage
    {
        public string FullName { get; set; }
    }

    public class NewMessageHandler
    {
        public void Handle(NewMessage message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
    }
}
