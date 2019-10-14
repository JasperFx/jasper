using System;
using Shouldly;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class AzureServiceBusOptionsTests
    {
        [Theory]
        [InlineData("azureservicebus://conn1/routingkey/one")]
        [InlineData("azureservicebus://conn1")]
        public void throw_with_invalid_uri_value(string uriString)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => { new AzureServiceBusOptions().For(uriString); });
        }
    }
}
