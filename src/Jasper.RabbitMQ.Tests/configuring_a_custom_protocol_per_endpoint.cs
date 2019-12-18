using System.Linq;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.RabbitMQ.Internal;
using LamarCodeGeneration.Util;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class configuring_a_custom_protocol_per_endpoint
    {
        [Fact]
        public void can_register_a_custom_protocol_per_endpoint()
        {
            using (var host = JasperHost.For<CustomProtocolApp>())
            {
                var endpoint = host.Get<IMessagingRoot>().Options.Endpoints.As<TransportCollection>()
                    .AllEndpoints().OfType<RabbitMqEndpoint>().Single();

                endpoint.Protocol.ShouldBeOfType<CustomProtocol>();
            }
        }

        public class CustomProtocolApp : JasperOptions
        {
            public CustomProtocolApp()
            {
                Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.AutoProvision = true;
                });

                Endpoints.ListenToRabbitQueue("queue1").Protocol<CustomProtocol>();
            }
        }

        public class CustomProtocol : DefaultRabbitMqProtocol
        {

        }
    }
}
