using System;
using System.Diagnostics;
using Jasper.AzureServiceBus.Internal;
using Jasper.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class AzureServiceBusEndpointTester
    {
        [Fact]
        public void default_protocol_is_the_default_protocol()
        {
            var endpoint = new AzureServiceBusEndpoint();
            endpoint.Protocol.ShouldBeOfType<DefaultAzureServiceBusProtocol>();
        }

        [Fact]
        public void parse_non_durable_uri()
        {
            var endpoint = new AzureServiceBusEndpoint();
            endpoint.Parse(new Uri("asb://subscription/sub1/topic/key1"));

            endpoint.Mode.ShouldBe(EndpointMode.BufferedInMemory);
            endpoint.SubscriptionName.ShouldBe("sub1");
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new AzureServiceBusEndpoint();
            endpoint.Parse(new Uri("asb://subscription/sub1/topic/key1/durable"));

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.SubscriptionName.ShouldBe("sub1");
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri_with_only_queue()
        {
            var endpoint = new AzureServiceBusEndpoint();
            endpoint.Parse(new Uri("asb://queue/q1/durable"));

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.QueueName.ShouldBe("q1");
        }

        [Fact]
        public void build_uri_for_subscription_and_topic()
        {
            new AzureServiceBusEndpoint
                {
                    SubscriptionName = "ex1",
                    TopicName = "key1"
                }
                .Uri.ShouldBe(new Uri("asb://subscription/ex1/topic/key1"));
        }

        [Fact]
        public void build_uri_for_queue_only()
        {
            new AzureServiceBusEndpoint
                {
                    QueueName = "foo"
                }
                .Uri.ShouldBe(new Uri("asb://queue/foo"));
        }


        [Fact]
        public void build_uri_for_queue_only_and_durable()
        {
            new AzureServiceBusEndpoint
                {
                    QueueName = "foo",
                    Mode = EndpointMode.Durable
                }
                .ReplyUri().ShouldBe(new Uri("asb://queue/foo/durable"));
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new AzureServiceBusEndpoint
                {
                    SubscriptionName = "ex1",
                    TopicName = "key1"
                }
                .ReplyUri().ShouldBe(new Uri("asb://topic/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            new AzureServiceBusEndpoint
            {
                SubscriptionName = "ex1",
                TopicName = "key1",
                Mode = EndpointMode.Durable
            }.ReplyUri().ShouldBe(new Uri("asb://topic/key1/durable"));


        }

    }
}
