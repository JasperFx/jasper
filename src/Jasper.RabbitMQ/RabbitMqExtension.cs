using Jasper.Configuration;
using Jasper.RabbitMQ;
using Jasper.RabbitMQ.Internal;
using LamarCodeGeneration.Util;
using RabbitMQ.Client;

[assembly: JasperModule(typeof(RabbitMqExtension))]

namespace Jasper.RabbitMQ
{
    public class RabbitMqExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Endpoints.As<TransportCollection>().Add(new RabbitMqTransport());
        }
    }
}
