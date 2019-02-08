using System;
using Jasper.RabbitMQ;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class EndpointTests
    {
        [Fact]
        public void read_host_from_connection_string()
        {
            var endpoint = new Endpoint("host=server1;queue=foo");
            endpoint.Host.ShouldBe("server1");
        }

        [Fact]
        public void default_port_is_5672()
        {
            var endpoint = new Endpoint("host=server1;queue=foo");
            endpoint.Port.ShouldBe(5672);
        }

        [Fact]
        public void read_port()
        {
            var endpoint = new Endpoint("host=server1;port=5673;queue=foo");
            endpoint.Port.ShouldBe(5673);
        }

        [Fact]
        public void queue_is_required()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("host=server1;port=5673");
            });
        }

        [Fact]
        public void host_is_required()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("port=5673;queue=foo");
            });
        }

        [Fact]
        public void unrecognized_key_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new Endpoint("port=5673;queue=foo;host=server3;unknown=four");
            });
        }

        [Fact]
        public void read_queue()
        {
            var endpoint = new Endpoint("host=server1;port=5673;queue=foo");
            endpoint.Queue.ShouldBe("foo");
        }

        [Fact]
        public void read_exchange()
        {
            var endpoint = new Endpoint("host=server1;port=5673;queue=foo;ExchangeName=bar");
            endpoint.ExchangeName.ShouldBe("bar");
        }

        [Fact]
        public void default_envelope_mapping()
        {
            var endpoint = new Endpoint("host=server1;queue=foo");
            endpoint.EnvelopeMapping.ShouldBeOfType<DefaultEnvelopeMapper>();
        }

        [Fact]
        public void default_is_durable_is_false()
        {
            var endpoint = new Endpoint("host=server1;queue=foo");
            endpoint.Durable.ShouldBeFalse();
        }

        [Fact]
        public void read_durable()
        {
            var endpoint = new Endpoint("host=server1;port=5674;durable=true;queue=foo");
            endpoint.Durable.ShouldBeTrue();
        }

        [Fact]
        public void default_exchange_type_is_direct()
        {
            var endpoint = new Endpoint("host=server1;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Direct);
        }

        [Fact]
        public void read_exchange_type()
        {
            var endpoint = new Endpoint("host=server1;ExchangeType=Fanout;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Fanout);
        }

        [Fact]
        public void read_topic()
        {
            var endpoint = new Endpoint("host=server2;topic=foo.bar;queue=foo");
            endpoint.Topic.ShouldBe("foo.bar");
        }
    }
}
