using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.DependencyInjection;

[assembly:JasperModule]

namespace Jasper.RabbitMQ
{
    public class RabbitMQTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<RabbitMQSettings>();
            registry.Services.AddTransient<ITransport, RabbitMQTransport>();
        }
    }
}
