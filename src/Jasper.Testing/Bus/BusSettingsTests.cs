using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Settings;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class BusSettingsTests
    {
        private readonly BusSettings theSettings = new BusSettings();

        [Fact]
        public void listen_to_loopback()
        {
            var queue = theSettings.ListenTo("loopback://incoming");

            queue.ShouldBeTheSameAs(theSettings.Loopback.Queues["incoming"]);
        }

        [Fact]
        public void do_not_allow_two_listening_ports()
        {
            theSettings.ListenTo("durable://localhost:2288/incoming");

            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                theSettings.ListenTo("durable://localhost:2289/incoming");
            });
        }

        [Fact]
        public void can_use_listen_to_multiple_times_with_the_same_schema_and_transport()
        {
            theSettings.ListenTo("durable://localhost:2288/incoming");
            theSettings.ListenTo("durable://localhost:2288/second");

            theSettings.Durable.Queues.Has("incoming").ShouldBeTrue();
            theSettings.Durable.Queues.Has("second").ShouldBeTrue();
            theSettings.Durable.Port.ShouldBe(2288);
        }

        [Fact]
        public void do_not_allow_multiple_transports_to_listen_on_the_same_port()
        {
            theSettings.ListenTo("durable://localhost:2288/incoming");

            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                theSettings.ListenTo("tcp://localhost:2288/second");
            });

        }

        [Fact]
        public void listen_to_durable()
        {
            var queue = theSettings.ListenTo("durable://localhost:2288/incoming");

            queue.ShouldBeTheSameAs(theSettings.Durable.Queues["incoming"]);
            theSettings.Durable.Port.ShouldBe(2288);
        }

        [Fact]
        public void listen_to_lightweight()
        {
            var queue = theSettings.ListenTo("tcp://localhost:2299/one");

            queue.ShouldBeTheSameAs(theSettings.Lightweight.Queues["one"]);
            theSettings.Lightweight.Port.ShouldBe(2299);
        }

        [Fact]
        public void listen_to_an_unknown_transport()
        {
            var queue = theSettings.ListenTo("rabbitmq://server/one");

            queue.ShouldBeTheSameAs(theSettings.Listeners.Single());

            theSettings.Listeners.Single().Uri
                .ShouldBe("rabbitmq://server/one".ToUri());
        }

        [Fact]
        public void listen_to_an_unknown_transport_is_idempotent()
        {
            var queue = theSettings.ListenTo("rabbitmq://server/one");
            var queue2 = theSettings.ListenTo("rabbitmq://server/one");

            queue.ShouldBeTheSameAs(queue2);

            queue.ShouldBeTheSameAs(theSettings.Listeners.Single());

            theSettings.Listeners.Single().Uri
                .ShouldBe("rabbitmq://server/one".ToUri());
        }

        [Fact]
        public void SendTo_is_cached()
        {
            var subscriber1 = theSettings.SendTo("tcp://localhost:2299/one");
            var subscriber2 = theSettings.SendTo("tcp://localhost:2299/one");

            subscriber1.ShouldBeSameAs(subscriber2);
        }

        [Fact]
        public async Task applies_lookups_to_listeners_happy_path()
        {
            theSettings.ListenTo("fake://one").MaximumParallelization(3);

            var lookups = new UriAliasLookup(new IUriLookup[0]);

            lookups.SetAlias("fake://one", "tcp://localhost:2222/incoming");



            await theSettings.ApplyLookups(lookups);

            theSettings.Lightweight.Port.ShouldBe(2222);
            theSettings.Lightweight.Queues.Has("incoming").ShouldBeTrue();
            theSettings.Lightweight.Queues["incoming"].Parallelization.ShouldBe(3);
        }


        [Fact]
        public async Task applies_lookups_to_senders()
        {
            theSettings.SendTo("fake://one");

            var lookups = new UriAliasLookup(new IUriLookup[0]);

            lookups.SetAlias("fake://one", "tcp://server:2222");

            await theSettings.ApplyLookups(lookups);

            theSettings.KnownSubscribers.Single()
                .Uri.ShouldBe("tcp://server:2222".ToUri());


        }

    }
}
