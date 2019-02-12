using System;
using Jasper.Settings;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqSettingsExtensions
    {
        public static void AddRabbitMqHost(this JasperSettings settings, string host, int port = 5672)
        {
            var connectionString = $"host={host};port={port}";

            settings.Alter<RabbitMqSettings>(s => s.Connections.Add(host, connectionString));
        }

        /// <summary>
        /// Register configuration of the Rabbit MQ transport
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void ConfigureRabbitMq(this JasperSettings settings, Action<RabbitMqSettings> configure)
        {
            settings.Alter(configure);
        }

        /// <summary>
        /// Register configuration of the Rabbit MQ transport
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void ConfigureRabbitMq(this JasperSettings settings, Action<WebHostBuilderContext, RabbitMqSettings> configure)
        {
            settings.Alter(configure);
        }
    }
}
