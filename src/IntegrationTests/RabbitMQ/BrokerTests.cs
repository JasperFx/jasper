using Jasper.RabbitMQ;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class BrokerTests
    {
        [Fact]
        public void read_port_and_host_from_uri()
        {
            var broker = new Broker("rabbitmq://server2:5672".ToUri());

            broker.ConnectionFactory.Port.ShouldBe(5672);
            broker.ConnectionFactory.HostName.ShouldBe("server2");
        }
    }
}
