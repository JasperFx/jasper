using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

[assembly: JasperModule(typeof(RabbitMqExtension))]

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            // this will force the transport collection
            // to add Rabbit MQ if it does not alreay
            // exist
            options.Endpoints.RabbitMqTransport();

        }
    }
}
