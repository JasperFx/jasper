using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.RabbitMQ.Internal;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(RabbitMqTransportExtension))]

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<RabbitMqSettings>();
            registry.Services.AddSingleton<ITransport, RabbitMqTransport>();
        }
    }
}
