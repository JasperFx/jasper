using System;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using Jasper.Util;
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
            endpoint.Parse(new Uri("rabbitmq://exchange/exchange1/routing/key1"));

            endpoint.Mode.ShouldBe(EndpointMode.BufferedInMemory);
            endpoint.ExchangeName.ShouldBe("exchange1");
            endpoint.RoutingKey.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new RabbitMqEndpoint();
            endpoint.Parse(new Uri("rabbitmq://exchange/exchange1/routing/key1/durable"));

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.ExchangeName.ShouldBe("exchange1");
            endpoint.RoutingKey.ShouldBe("key1");
        }

        [Fact]
        public void parse_durable_uri_with_only_queue()
        {
            var endpoint = new RabbitMqEndpoint();
            endpoint.Parse(new Uri("rabbitmq://queue/q1/durable"));

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.QueueName.ShouldBe("q1");
        }

        [Fact]
        public void build_uri_for_exchange_and_routing()
        {
            new RabbitMqEndpoint
                {
                    ExchangeName = "ex1",
                    RoutingKey = "key1"
                }
                .Uri.ShouldBe(new Uri("rabbitmq://exchange/ex1/routing/key1"));
        }

        [Fact]
        public void build_uri_for_queue_only()
        {
            new RabbitMqEndpoint
                {
                    QueueName = "foo"
                }
                .Uri.ShouldBe(new Uri("rabbitmq://queue/foo"));
        }


        [Fact]
        public void build_uri_for_queue_only_and_durable()
        {
            new RabbitMqEndpoint
                {
                    QueueName = "foo",
                    Mode = EndpointMode.Durable
                }
                .ReplyUri().ShouldBe(new Uri("rabbitmq://queue/foo/durable"));
        }

        [Fact]
        public void build_uri_for_exchange_only()
        {
            new RabbitMqEndpoint()
            {
                ExchangeName = "ex2"

            }.Uri.ShouldBe("rabbitmq://exchange/ex2".ToUri());
        }

        [Fact]
        public void build_uri_for_exchange_and_topics()
        {
            new RabbitMqEndpoint()
            {
                ExchangeName = "ex2"

            }.Uri.ShouldBe("rabbitmq://exchange/ex2".ToUri());
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new RabbitMqEndpoint
                {
                    ExchangeName = "ex1",
                    RoutingKey = "key1"
                }
                .ReplyUri().ShouldBe(new Uri("rabbitmq://exchange/ex1/routing/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            new RabbitMqEndpoint
            {
                ExchangeName = "ex1",
                RoutingKey = "key1",
                Mode = EndpointMode.Durable
            }.ReplyUri().ShouldBe(new Uri("rabbitmq://exchange/ex1/routing/key1/durable"));


        }


    }
}
