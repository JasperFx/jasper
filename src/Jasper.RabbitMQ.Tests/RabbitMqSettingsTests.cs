using System;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqSettingsTests
    {
        [Fact]
        public void throw_with_invalid_uri_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                new RabbitMqOptions().For("rabbitmq://conn1/subscription/one");
            });
        }

        [Fact]
        public void throw_with_wrong_protocol()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                new RabbitMqOptions().For("wrong://conn1/subscription/one");
            });
        }

        [Fact]
        public void configure_all_endpoints_by_connection_name()
        {
            var settings = new RabbitMqOptions();

            settings.Connections.Add("conn1", "host=localhost");
            settings.Connections.Add("conn2", "host=localhost");

            var e1 = settings.For("rabbitmq://conn1/queue/one");
            var e2 = settings.For("rabbitmq://conn1/queue/two");
            var e3 = settings.For("rabbitmq://conn2/queue/one");
            var e4 = settings.For("rabbitmq://conn2/queue/two");
            var e5 = settings.For("rabbitmq://conn2/queue/three");

            settings.ConfigureEndpointsForConnection("conn1", e => e.Port = 1);

            e1.Port.ShouldBe(1);
            e2.Port.ShouldBe(1);

            e3.Port.ShouldBe(5672);
            e4.Port.ShouldBe(5672);
            e5.Port.ShouldBe(5672);
        }
    }
}
