using System;
using Jasper.ConfluentKafka;
using Shouldly;
using Xunit;

namespace Jasper.Kafka.Tests
{
    public class KafkaEndpointTester
    {
        [Fact]
        public void default_protocol_is_the_default_protocol()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Protocol.ShouldBeOfType<DefaultKafkaProtocol>();
        }

        [Fact]
        public void parse_non_durable_uri()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Parse(new Uri("ckafka://subscription/sub1/topic/key1"));

            endpoint.IsDurable.ShouldBeFalse();
            endpoint.SubscriptionName.ShouldBe("sub1");
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Parse(new Uri("ckafka://subscription/sub1/topic/key1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.SubscriptionName.ShouldBe("sub1");
            endpoint.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri_with_only_queue()
        {
            var endpoint = new KafkaEndpoint();
            endpoint.Parse(new Uri("ckafka://queue/q1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.QueueName.ShouldBe("q1");
        }

        [Fact]
        public void build_uri_for_subscription_and_topic()
        {
            new KafkaEndpoint
                {
                    SubscriptionName = "ex1",
                    TopicName = "key1"
                }
                .Uri.ShouldBe(new Uri("ckafka://subscription/ex1/topic/key1"));
        }

        [Fact]
        public void build_uri_for_queue_only()
        {
            new KafkaEndpoint
                {
                    QueueName = "foo"
                }
                .Uri.ShouldBe(new Uri("ckafka://queue/foo"));
        }


        [Fact]
        public void build_uri_for_queue_only_and_durable()
        {
            new KafkaEndpoint
                {
                    QueueName = "foo",
                    IsDurable = true
                }
                .ReplyUri().ShouldBe(new Uri("ckafka://queue/foo/durable"));
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new KafkaEndpoint
                {
                    SubscriptionName = "ex1",
                    TopicName = "key1"
                }
                .ReplyUri().ShouldBe(new Uri("ckafka://topic/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            new KafkaEndpoint
            {
                SubscriptionName = "ex1",
                TopicName = "key1",
                IsDurable = true
            }.ReplyUri().ShouldBe(new Uri("ckafka://topic/key1/durable"));


        }

    }
}
