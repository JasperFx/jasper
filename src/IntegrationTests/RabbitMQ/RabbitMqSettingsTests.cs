using System;
using Jasper.RabbitMQ;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class RabbitMqSettingsTests
    {
        [Fact]
        public void throw_with_invalid_uri_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                new RabbitMqSettings().For("rabbitmq://conn1/subscription/one");
            });
        }

        [Fact]
        public void throw_with_wrong_protocol()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                new RabbitMqSettings().For("wrong://conn1/subscription/one");
            });
        }
    }
}
