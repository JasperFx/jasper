using System;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace MessagingTests
{
    public class TransportUriTests
    {
        [Theory]
        [InlineData("rabbitmq://conn1/topic/foo", "rabbitmq", "conn1", null, "foo", false)]
        [InlineData("rabbitmq://conn1/durable/topic/foo", "rabbitmq", "conn1", null, "foo", true)]
        [InlineData("rabbitmq://conn1/durable/queue/foo", "rabbitmq", "conn1", "foo", null, true)]
        [InlineData("rabbitmq://conn2/durable/queue/foo", "rabbitmq", "conn2", "foo", null, true)]
        [InlineData("azureservicebus://conn2/durable/queue/foo", "azureservicebus", "conn2", "foo", null, true)]
        public void read_uri(string uriString, string protocol, string connectionName, string queue, string topic, bool durable)
        {
            var uri = new TransportUri(uriString);

            uri.Protocol.ShouldBe(protocol);
            uri.ConnectionName.ShouldBe(connectionName);
            uri.QueueName.ShouldBe(queue);
            uri.TopicName.ShouldBe(topic);
            uri.Durable.ShouldBe(durable);
        }

        [Theory]
        [InlineData("rabbitmq://conn1/topic/foo", "rabbitmq", "conn1", null, "foo", false)]
        [InlineData("rabbitmq://conn1/durable/topic/foo", "rabbitmq", "conn1", null, "foo", true)]
        [InlineData("rabbitmq://conn1/durable/queue/foo", "rabbitmq", "conn1", "foo", null, true)]
        [InlineData("rabbitmq://conn2/durable/queue/foo", "rabbitmq", "conn2", "foo", null, true)]
        [InlineData("azureservicebus://conn2/durable/queue/foo", "azureservicebus", "conn2", "foo", null, true)]
        public void generate_uri(string uriString, string protocol, string connectionName, string queue, string topic, bool durable)
        {
            var uri = new TransportUri(protocol, connectionName, durable, queueName:queue, topicName:topic);

            uri.ToUri().ShouldBe(uriString.ToUri());
        }

        [Fact]
        public void read_subscription()
        {
            var uri = new TransportUri("azureservicebus://conn1/subscription/one");
            uri.SubscriptionName.ShouldBe("one");
        }

        [Fact]
        public void write_uri_with_subscription()
        {
            var uri = new TransportUri("azureservicebus", "conn1", true, subscriptionName:"one");
            uri.ToUri().ShouldBe(new Uri("azureservicebus://conn1/durable/subscription/one"));
        }

        [Fact]
        public void replace_connection()
        {
            var uri = new TransportUri("rabbitmq://conn1/topic/foo");
            var uri2 = uri.ReplaceConnection("conn2");

            uri2.ToUri().ShouldBe("rabbitmq://conn2/topic/foo".ToUri());
        }

        [Theory]
        [InlineData("rabbitmq://conn1/topic/*", true)]
        [InlineData("rabbitmq://conn1/topic/foo", false)]
        [InlineData("rabbitmq://conn1/queue/foo", false)]
        public void is_message_type_topic(string uriString, bool isMessageTypeTopic)
        {
            new TransportUri(uriString).IsMessageSpecificTopic().ShouldBe(isMessageTypeTopic);
        }

    }
}
