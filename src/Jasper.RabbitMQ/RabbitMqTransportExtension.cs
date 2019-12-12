using Jasper.Configuration;
using Jasper.RabbitMQ;
using Jasper.RabbitMQ.Internal;
using LamarCodeGeneration.Util;

[assembly: JasperModule(typeof(RabbitMqTransportExtension))]

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransportExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Endpoints.As<TransportCollection>().Add(new RabbitMqTransport());
        }
    }
}
