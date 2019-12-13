using Jasper.RabbitMQ.Internal;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqTransportTester
    {
        [Fact]
        public void automatic_recovery_is_try_by_default()
        {
            var transport = new RabbitMqTransport();
            transport.ConnectionFactory.AutomaticRecoveryEnabled.ShouldBeTrue();
        }

        [Fact]
        public void auto_provision_is_false_by_default()
        {
            var transport = new RabbitMqTransport();
            transport.AutoProvision.ShouldBeFalse();
        }
    }
}
