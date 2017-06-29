using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.Bus.Transports;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class using_uri_lookups : IntegrationContext
    {
        [Fact]
        public void ChannelGraph_is_corrected_by_the_lookups()
        {
            with(_ =>
            {
                _.Services.For<IUriLookup>().Add<FakeUriLookup>();
                _.Channel("fake://one");
            });

            Channels.Where(x => x.Uri.Scheme == "memory").Any(x => x.Uri == "memory://one".ToUri())
                .ShouldBeTrue();


        }

        [Fact]
        public void ChannelGraph_can_use_the_alias_to_give_you_the_same_node()
        {
            with(_ =>
            {
                _.Services.For<IUriLookup>().Add<FakeUriLookup>();
                _.Channel("fake://one");
            });

            Channels["fake://one"].ShouldBeSameAs(Channels["memory://one"]);

            Channels["fake://one"].Uri.ShouldBe("memory://one".ToUri());
        }

        [Fact]
        public async Task send_via_the_alias_and_messages_actually_get_there()
        {
            var tracker = new MessageTracker();

            with(_ =>
            {
                _.Services.For<MessageTracker>().Use(tracker);
                _.Services.For<IUriLookup>().Add<FakeUriLookup>();
                _.SendMessage<Message1>().To("fake://one");
                _.ListenForMessagesFrom("fake://one");
            });

            var waiter = tracker.WaitFor<Message1>();

            await Bus.Send(new Message1());

            var envelope = await waiter;

            envelope.Destination.ShouldBe("memory://one".ToUri());
        }

        [Fact]
        public async Task send_via_the_alias_and_messages_actually_get_there_2()
        {
            var tracker = new MessageTracker();

            with(_ =>
            {
                _.Services.For<MessageTracker>().Use(tracker);
                _.Services.For<IUriLookup>().Add<FakeUriLookup>();
                _.ListenForMessagesFrom("fake://one");
            });

            var waiter = tracker.WaitFor<Message1>();

            await Bus.Send("fake://one".ToUri(), new Message1());

            var envelope = await waiter;

            envelope.Destination.ShouldBe("memory://one".ToUri());
        }

        [Fact]
        public void can_use_config_lookups()
        {
            with(_ =>
            {
                _.Configuration.AddInMemoryCollection(
                    new Dictionary<string, string> {{"outgoing", "lq.tcp://server1:2200/outgoing"}});

                _.SendMessage<Message1>().To("config://outgoing");
            });

            Channels["config://outgoing"].Uri.ShouldBe("lq.tcp://server1:2200/outgoing".ToUri());
            Channels.HasChannel("lq.tcp://server1:2200/outgoing".ToUri()).ShouldBeTrue();
        }
    }

    public class FakeUriLookup : IUriLookup
    {
        public string Protocol { get; } = "fake";
        public Uri Lookup(Uri original)
        {
            return $"memory://{original.Host}".ToUri();
        }
    }
}
