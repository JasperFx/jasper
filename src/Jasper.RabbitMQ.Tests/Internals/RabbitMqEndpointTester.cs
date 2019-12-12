using System;
using Jasper.RabbitMQ.Internal;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests.Internals
{
    public class RabbitMqEndpointTester
    {
        [Fact]
        public void parse_non_durable_uri()
        {
            var endpoint = new RabbitMqEndpoint();
            endpoint.Parse(new Uri("rabbitmq://exchange1/key1"));

            endpoint.IsDurable.ShouldBeFalse();
            endpoint.ExchangeName.ShouldBe("exchange1");
            endpoint.RoutingKey.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new RabbitMqEndpoint();
            endpoint.Parse(new Uri("rabbitmq://exchange1/key1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.ExchangeName.ShouldBe("exchange1");
            endpoint.RoutingKey.ShouldBe("key1");
        }

        [Fact]
        public void build_uri_on_construction()
        {
            new RabbitMqEndpoint("ex1", "key1")
                .Uri.ShouldBe(new Uri("rabbitmq://ex1/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new RabbitMqEndpoint("ex1", "key1")
                .ReplyUri().ShouldBe(new Uri("rabbitmq://ex1/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            var endpoint = new RabbitMqEndpoint("ex1", "key1");
            endpoint.IsDurable = true;

            endpoint
                .ReplyUri().ShouldBe(new Uri("rabbitmq://ex1/key1/durable"));


        }
    }
}
