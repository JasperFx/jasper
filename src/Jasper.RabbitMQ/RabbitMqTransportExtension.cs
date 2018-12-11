using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(RabbitMqTransportExtension))]

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransportExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.Settings.Require<RabbitMqSettings>();
            registry.Services.AddTransient<ITransport, RabbitMqTransport>();
        }
    }
}
