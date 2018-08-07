using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.DependencyInjection;

[assembly:JasperModule(typeof(Jasper.RabbitMQ.RabbitMqTransportExtension))]

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<RabbitMqSettings>();
            registry.Services.AddTransient<ITransport, RabbitMqTransport>();
        }
    }
}
