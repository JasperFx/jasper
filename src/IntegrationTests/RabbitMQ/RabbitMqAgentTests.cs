using System;
using Jasper.RabbitMQ;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class RabbitMqAgentTests
    {
        [Fact]
        public void parse_a_uri_with_no_port()
        {
            var agent = new RabbitMqAgent("rabbitmq://localhost/something");
            agent.ConnectionFactory.Port.ShouldBe(5672);
        }

        [Fact]
        public void parse_a_uri_with_a_port()
        {
            var agent = new RabbitMqAgent("rabbitmq://localhost:5673/something");
            agent.ConnectionFactory.Port.ShouldBe(5673);
        }

        [Fact]
        public void throws_if_protocol_is_not_rabbitmq()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() =>
            {
                var rabbitMqAgent = new RabbitMqAgent("tcp://localhost:5000");
            });
        }

        [Theory]
        [InlineData("rabbitmq://localhost")]
        [InlineData("rabbitmq://localhost/durable")]
        [InlineData("rabbitmq://localhost/durable/direct")]
        public void throws_if_there_is_no_queue(string uri)
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => { new RabbitMqAgent(uri); });
        }


        [Theory]
        [InlineData("rabbitmq://localhost/one", false, "", ExchangeType.direct, "one")]
        [InlineData("rabbitmq://localhost/durable/two/", true, "", ExchangeType.direct, "two")]
        [InlineData("rabbitmq://localhost/durable/durable/", true, "", ExchangeType.direct, "durable")]
        [InlineData("rabbitmq://localhost/durable/fanout/three", true, "", ExchangeType.fanout, "three")]
        [InlineData("rabbitmq://localhost/fanout/three", false, "", ExchangeType.fanout, "three")]
        [InlineData("rabbitmq://localhost/durable/fanout/exchange1/three", true, "exchange1", ExchangeType.fanout, "three")]
        [InlineData("rabbitmq://localhost/fanout/exchange1/three", false, "exchange1", ExchangeType.fanout, "three")]
        public void parse_uri_patterns(
            string uri,
            bool isDurable,
            string exchangeName,
            ExchangeType exchangeType,
            string queueName)
        {
            var agent = new RabbitMqAgent(uri);
            agent.IsDurable.ShouldBe(isDurable);
            agent.ExchangeName.ShouldBe(exchangeName);
            agent.ExchangeType.ShouldBe(exchangeType);
            agent.QueueName.ShouldBe(queueName);
        }
    }
}
