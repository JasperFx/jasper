using System;
using Jasper.Configuration;
using Jasper.ConfluentKafka;
using Shouldly;
using Xunit;

namespace Jasper.Kafka.Tests
{
    public class KafkaEndpointTester
    {
        [Fact]
        public void parse_non_durable_uri()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Parse(new Uri("kafka://topic/key1"));

            endpoint.Mode.ShouldBe(EndpointMode.Queued);
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Parse(new Uri("kafka://topic/key1/durable"));

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void build_uri_for_subscription_and_topic()
        {
            new KafkaEndpoint
            {
                TopicName = "key1"
            }
                .Uri.ShouldBe(new Uri("kafka://topic/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new KafkaEndpoint
            {
                TopicName = "key1"
            }
                .ReplyUri().ShouldBe(new Uri("kafka://topic/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            new KafkaEndpoint
            {
                TopicName = "key1",
                Mode = EndpointMode.Durable
            }.ReplyUri().ShouldBe(new Uri("kafka://topic/key1/durable"));
        }

    }
}
