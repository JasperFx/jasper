using System;
using System.Collections.Generic;
using Jasper.RabbitMQ;
using Jasper.Util;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.RabbitMQ
{
    public class EndpointTests
    {
        private readonly ITestOutputHelper _output;

        public EndpointTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void read_host_from_connection_string()
        {
            var endpoint = new Endpoint("default", "host=server1;queue=foo");
            endpoint.Host.ShouldBe("server1");
        }

        [Fact]
        public void default_port_is_5672()
        {
            var endpoint = new Endpoint("default", "host=server1;queue=foo");
            endpoint.Port.ShouldBe(5672);
        }

        [Fact]
        public void read_port()
        {
            var endpoint = new Endpoint("default", "host=server1;port=5673;queue=foo");
            endpoint.Port.ShouldBe(5673);
        }

        [Fact]
        public void queue_is_required()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("default", "host=server1;port=5673");
            });
        }

        [Fact]
        public void host_is_required()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("default", "port=5673;queue=foo");
            });
        }

        [Fact]
        public void unrecognized_key_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("default", "port=5673;queue=foo;host=server3;unknown=four");
            });
        }

        [Fact]
        public void read_queue()
        {
            var endpoint = new Endpoint("default", "host=server1;port=5673;queue=foo");
            endpoint.Queue.ShouldBe("foo");
        }

        [Fact]
        public void read_exchange()
        {
            var endpoint = new Endpoint("default", "host=server1;port=5673;queue=foo;ExchangeName=bar");
            endpoint.ExchangeName.ShouldBe("bar");
        }

        [Fact]
        public void default_envelope_mapping()
        {
            var endpoint = new Endpoint("default", "host=server1;queue=foo");
            endpoint.EnvelopeMapping.ShouldBeOfType<DefaultEnvelopeMapper>();
        }

        [Fact]
        public void default_is_durable_is_false()
        {
            var endpoint = new Endpoint("default", "host=server1;queue=foo");
            endpoint.Durable.ShouldBeFalse();
        }

        [Fact]
        public void read_durable()
        {
            var endpoint = new Endpoint("default", "host=server1;port=5674;durable=true;queue=foo");
            endpoint.Durable.ShouldBeTrue();
        }

        [Fact]
        public void default_exchange_type_is_direct()
        {
            var endpoint = new Endpoint("default", "host=server1;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Direct);
        }

        [Fact]
        public void read_exchange_type()
        {
            var endpoint = new Endpoint("default", "host=server1;ExchangeType=Fanout;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Fanout);
        }

        [Fact]
        public void read_topic()
        {
            var endpoint = new Endpoint("default", "host=server2;topic=foo.bar;queue=foo");
            endpoint.Topic.ShouldBe("foo.bar");
        }

                [Theory]
        [InlineData("rabbitmq://localhost")]
        [InlineData("rabbitmq://localhost/durable")]
        [InlineData("rabbitmq://localhost/durable/direct")]
        public void throws_if_there_is_no_queue(string uri)
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => { new Endpoint(uri.ToUri()); });
        }


        [Theory]
        [InlineData("rabbitmq://localhost/one", false, "", ExchangeType.Direct, "one")]
        [InlineData("rabbitmq://localhost/durable/two/", true, "", ExchangeType.Direct, "two")]
        [InlineData("rabbitmq://localhost/durable/durable/", true, "", ExchangeType.Direct, "durable")]
        [InlineData("rabbitmq://localhost/durable/fanout/three", true, "", ExchangeType.Fanout, "three")]
        [InlineData("rabbitmq://localhost/fanout/three", false, "", ExchangeType.Fanout, "three")]
        [InlineData("rabbitmq://localhost/durable/fanout/exchange1/three", true, "exchange1", ExchangeType.Fanout,
            "three")]
        [InlineData("rabbitmq://localhost/fanout/exchange1/three", false, "exchange1", ExchangeType.Fanout, "three")]
        public void parse_uri_patterns(
            string uri,
            bool isDurable,
            string exchangeName,
            ExchangeType exchangeType,
            string queueName)
        {
            var agent = new Endpoint(uri.ToUri());
            agent.Durable.ShouldBe(isDurable);
            agent.ExchangeName.ShouldBe(exchangeName);
            agent.ExchangeType.ShouldBe(exchangeType);
            agent.Queue.ShouldBe(queueName);
        }

        [Theory]
        [InlineData("rabbitmq://localhost:5672/direct/one")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/two")]
        [InlineData("rabbitmq://localhost:5672/durable/direct/four")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/three")]
        [InlineData("rabbitmq://localhost:5672/fanout/three")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/exchange1/three")]
        [InlineData("rabbitmq://localhost:5672/fanout/exchange1/three")]
        public void generate_full_uri(
            string uri)
        {
            var agent = new Endpoint(uri.ToUri());
            agent.ToFullUri().ShouldBe(uri.ToUri());
        }

        [Fact]
        public void parse_a_uri_with_a_port()
        {
            var agent = new Endpoint("rabbitmq://localhost:5673/something".ToUri());
            agent.BrokerUri.Port.ShouldBe(5673);
        }

        [Fact]
        public void parse_a_uri_with_no_port()
        {
            var agent = new Endpoint("rabbitmq://localhost/something".ToUri());
            agent.BrokerUri.Port.ShouldBe(5672);
        }

        [Fact]
        public void throws_if_protocol_is_not_rabbitmq()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() =>
            {
                var rabbitMqAgent = new Endpoint("tcp://localhost:5000".ToUri());
            });
        }
    }


}
