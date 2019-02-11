using System;
using System.Collections.Generic;
using Jasper.Messaging.Transports;
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
        private readonly TransportUri theUri = new TransportUri("rabbitmq", "default", false, "queue1");

        public EndpointTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void read_host_from_connection_string()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;");
            endpoint.Host.ShouldBe("server1");
        }

        [Fact]
        public void default_port_is_5672()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1");
            endpoint.Port.ShouldBe(5672);
        }

        [Fact]
        public void read_port()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;port=5673");
            endpoint.Port.ShouldBe(5673);
        }


        [Fact]
        public void host_is_required()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new RabbitMqEndpoint(theUri, "port=5673");
            });
        }

        [Fact]
        public void unrecognized_key_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new RabbitMqEndpoint(theUri, "port=5673;host=server3;unknown=four");
            });
        }

        [Fact]
        public void read_queue()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;port=5673;queue=foo");
            endpoint.Queue.ShouldBe("foo");
        }

        [Fact]
        public void read_exchange()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;port=5673;queue=foo;ExchangeName=bar");
            endpoint.ExchangeName.ShouldBe("bar");
        }

        [Fact]
        public void default_envelope_mapping()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;queue=foo");
            endpoint.Protocol.ShouldBeOfType<DefaultRabbitMqProtocol>();
        }

        [Fact]
        public void default_is_durable_is_false()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;queue=foo");
            endpoint.Durable.ShouldBeFalse();
        }

        [Fact]
        public void read_durable()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;port=5674;durable=true;queue=foo");
            endpoint.Durable.ShouldBeTrue();
        }

        [Fact]
        public void default_exchange_type_is_direct()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Direct);
        }

        [Fact]
        public void read_exchange_type()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;ExchangeType=Fanout;queue=foo");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Fanout);
        }

        [Fact]
        public void read_topic()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server2;topic=foo.bar;queue=foo");
            endpoint.Topic.ShouldBe("foo.bar");
        }


    }


}
