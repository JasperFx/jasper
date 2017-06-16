using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{

    public class LocalSubscriptionsTester
    {
        public readonly BusSettings _settings = new BusSettings
        {
            Upstream = new Uri("memory://upstream"),
            Downstream = new Uri("memory://downstream"),
            Outbound = new Uri("memory://outbound")
        };
        private readonly ChannelGraph _graph;
        private readonly Uri _localReplyUri;
        private readonly IEnumerable<Subscription> _subscriptions;

        public LocalSubscriptionsTester()
        {
            _graph = new ChannelGraph
            {
                Name = "FooNode",
            };

            var requirement = new LocalSubscriptionRequirement(_settings.Upstream);
            requirement.AddType(typeof(FooMessage));
            requirement.AddType(typeof(BarMessage));

            _localReplyUri = _settings.Downstream;

            _graph.AddChannelIfMissing("fake2://2".ToUri()).Incoming = true;
            _graph.AddChannelIfMissing(_localReplyUri).Incoming = true;
            _graph.AddChannelIfMissing("fake1://1".ToUri()).Incoming = true;

            _subscriptions = requirement.Determine(_graph);
        }

        [Fact]
        public void should_set_the_receiver_uri_to_the_reply_uri_of_the_matching_transport()
        {
            _subscriptions.First().Receiver
                .ShouldBe(_localReplyUri);
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
            public Uri Downstream { get; set; }
            public Uri Upstream { get; set; }
        }

        public class FooMessage{}
        public class BarMessage{}
    }
}
