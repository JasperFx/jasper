using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.RabbitMQ;
using Jasper.RabbitMQ.Internal;
using Jasper.Settings;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(RabbitMqTransportExtension))]

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<RabbitMqOptions>();
            registry.Services.AddSingleton<ITransport, RabbitMqTransport>();
        }
    }
}
