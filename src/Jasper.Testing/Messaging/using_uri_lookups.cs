using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class using_uri_lookups : IntegrationContext
    {
        [Fact]
        public async Task using_the_config_lookup()
        {
            await with(_ =>
            {
                _.Configuration
                    .AddInMemoryCollection(new Dictionary<string, string> {{"invoicing", "durable://server2:2345"}});


                _.Transports.ListenForMessagesFrom("config://invoicing");
            });

            Runtime.Get<MessagingSettings>().Listeners.Any(x => x.Port == 2345).ShouldBeTrue();
        }


        [Fact]
        public async Task static_routing_rules_respect_the_uri_lookup()
        {
            await with(_ =>
            {
                _.Services.For<IUriLookup>().Use<FakeUriLookup>();
                _.Publish.Message<Message1>().To("fake://one");
            });

            var router = Runtime.Get<IMessageRouter>();
            var tracks = await router.Route(typeof(Message1));

            tracks.Single().Destination.ShouldBe("loopback://one".ToUri());
        }


        [Fact]
        public async Task listeners_are_corrected_by_the_lookups()
        {
            await with(_ =>
            {
                _.Services.For<IUriLookup>().Use<FakeUriLookup>();
                _.Transports.ListenForMessagesFrom("fake://one");
            });

            Runtime.Get<MessagingSettings>().Listeners.Contains("loopback://one".ToUri());



        }


        [Fact]
        public async Task send_via_the_alias_and_messages_actually_get_there()
        {
            var tracker = new MessageTracker();

            await with(_ =>
            {
                _.Services.AddSingleton(tracker);
                _.Services.For<IUriLookup>().Use<FakeUriLookup>();
                _.Publish.Message<Message1>().To("fake://one");
                _.Transports.ListenForMessagesFrom("fake://one");
            });

            var waiter = tracker.WaitFor<Message1>();

            await Bus.Send(new Message1());

            var envelope = await waiter;

            envelope.Destination.ShouldBe("loopback://one".ToUri());
        }

        [Fact]
        public async Task send_via_the_alias_and_messages_actually_get_there_2()
        {
            var tracker = new MessageTracker();

            await with(_ =>
            {
                _.Services.AddSingleton(tracker);
                _.Services.For<IUriLookup>().Use<FakeUriLookup>();
                _.Transports.ListenForMessagesFrom("fake://one");
            });

            var waiter = tracker.WaitFor<Message1>();

            await Bus.Send("fake://one".ToUri(), new Message1());

            var envelope = await waiter;

            envelope.Destination.ShouldBe("loopback://one".ToUri());
        }

    }

    public class FakeUriLookup : IUriLookup
    {
        public string Protocol { get; } = "fake";
        public Task<Uri[]> Lookup(Uri[] originals)
        {
            var actuals = originals.Select(x => $"loopback://{x.Host}".ToUri());

            return Task.FromResult(actuals.ToArray());
        }
    }
}
