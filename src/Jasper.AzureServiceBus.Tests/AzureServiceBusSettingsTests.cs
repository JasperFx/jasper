using System;
using Jasper.RabbitMQ;
using Shouldly;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class AzureServiceBusSettingsTests
    {
        [Fact]
        public void throw_with_invalid_uri_value()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                new AzureServiceBusSettings().For("azureservicebus://conn1/routingkey/one");
            });
        }
    }
}
