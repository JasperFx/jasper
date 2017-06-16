using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Subscriptions
{

    public class GroupedSubscriptionsTester
    {
        public readonly BusSettings _settings = new BusSettings
        {
            Upstream = new Uri("memory://upstream"),
            Incoming = new Uri("memory://incoming")
        };
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<Subscription> _subscriptions;

        public GroupedSubscriptionsTester()
        {
            _graph = new ChannelGraph { Name = "FooNode" };

            var requirement = new GroupSubscriptionRequirement(_settings.Upstream, _settings.Incoming);
            requirement.AddType(typeof(FooMessage));
            requirement.AddType(typeof(BarMessage));

            _subscriptions = requirement.Determine(_graph);
        }

        [Fact]
        public void should_set_the_receiver_uri_to_the_explicitly_chosen_uri()
        {
            _subscriptions.First().Receiver
                .ShouldBe(_settings.Incoming);
        }

        [Fact]
        public void sets_the_node_name_from_the_channel_graph()
        {
            _subscriptions.Select(x => x.NodeName).Distinct()
                .Single().ShouldBe(_graph.Name);
        }

        [Fact]
        public void should_set_the_source_uri_to_the_requested_source_from_settings()
        {
            _subscriptions.First().Source
                .ShouldBe(_settings.Upstream);
        }

        [Fact]
        public void should_add_a_subscription_for_each_type()
        {
            _subscriptions.Select(x => x.MessageType)
                .ShouldHaveTheSameElementsAs(typeof(FooMessage).GetFullName(), typeof(BarMessage).GetFullName());
        }

        public class BusSettings
        {
            public Uri Outbound { get; set; }
            public Uri Incoming { get; set; }
            public Uri Upstream { get; set; }
        }

        public class FooMessage { }
        public class BarMessage { }
    }
}
