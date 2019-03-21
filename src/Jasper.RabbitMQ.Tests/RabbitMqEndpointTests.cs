using System;
using Jasper.Messaging.Transports;
using Jasper.RabbitMQ.Internal;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqEndpointTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TransportUri theUri = new TransportUri("rabbitmq", "default", false, "queue1");

        public RabbitMqEndpointTests(ITestOutputHelper output)
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
        public void unrecognized_key_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var endpoint = new RabbitMqEndpoint(theUri, "port=5673;host=server3;unknown=four");
            });
        }

        [Fact]
        public void read_exchange()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;port=5673;ExchangeName=bar");
            endpoint.ExchangeName.ShouldBe("bar");
        }

        [Fact]
        public void default_envelope_mapping()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1");
            endpoint.Protocol.ShouldBeOfType<DefaultRabbitMqProtocol>();
        }

        [Fact]
        public void default_is_durable_is_false()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1");
            endpoint.TransportUri.Durable.ShouldBeFalse();
        }


        [Fact]
        public void default_exchange_type_is_direct()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Direct);
        }

        [Fact]
        public void read_exchange_type()
        {
            var endpoint = new RabbitMqEndpoint(theUri, "host=server1;ExchangeType=Fanout");
            endpoint.ExchangeType.ShouldBe(ExchangeType.Fanout);
        }


    }


}
