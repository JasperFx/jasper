using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;
using Jasper.Testing.Conneg;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{

    public class GroupedSubscriptionsTester
    {
        public readonly BusSettings _settings = new BusSettings
        {
            Upstream = new Uri("memory://upstream"),
            Incoming = new Uri("memory://incoming")
        };
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<Subscription> theSubscriptions;
        private SerializationGraph theSerialization;

        public GroupedSubscriptionsTester()
        {
            var serializers = new ISerializer[]{new NewtonsoftSerializer(new Jasper.Bus.BusSettings())};
            var readers = new IMediaReader[] {new FakeReader(typeof(FooMessage), "foo/special"),  };

            theSerialization =
                new SerializationGraph(new HandlerGraph(), serializers, readers, new List<IMediaWriter>());

            _graph = new ChannelGraph { Name = "FooNode" };

            var requirement = new GroupSubscriptionRequirement(_settings.Upstream, _settings.Incoming);
            requirement.AddType(typeof(FooMessage));
            requirement.AddType(typeof(BarMessage));

            theSubscriptions = requirement.Determine(_graph, theSerialization);
        }

        [Fact]
        public void should_specify_the_accepts_with_json_last()
        {
            theSubscriptions.First().Accepts.ShouldBe("foo/special,application/json");
            theSubscriptions.Last().Accepts.ShouldBe("application/json");
        }

        [Fact]
        public void should_set_the_message_type()
        {
            theSubscriptions.First().MessageType.ShouldBe(typeof(FooMessage).ToTypeAlias());
        }

        [Fact]
        public void should_set_the_receiver_uri_to_the_explicitly_chosen_uri()
        {
            theSubscriptions.First().Receiver
                .ShouldBe(_settings.Incoming);
        }

        [Fact]
        public void sets_the_node_name_from_the_channel_graph()
        {
            theSubscriptions.Select(x => x.NodeName).Distinct()
                .Single().ShouldBe(_graph.Name);
        }

        [Fact]
        public void should_set_the_source_uri_to_the_requested_source_from_settings()
        {
            theSubscriptions.First().Source
                .ShouldBe(_settings.Upstream);
        }

        [Fact]
        public void should_add_a_subscription_for_each_type()
        {
            theSubscriptions.Select(x => x.MessageType)
                .ShouldHaveTheSameElementsAs(typeof(FooMessage).ToTypeAlias(), typeof(BarMessage).ToTypeAlias());
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
