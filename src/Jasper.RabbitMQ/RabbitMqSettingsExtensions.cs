using System;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqSettingsExtensions
    {
        public static void AddRabbitMqHost(this SettingsGraph settings, string host, int port = 5672)
        {
            var connectionString = $"host={host};port={port}";

            settings.Alter<RabbitMqOptions>(s => s.Connections.Add(host, connectionString));
        }

        public static void AddRabbitMqConnection(this SettingsGraph settings, string connectionName, string connectionString)
        {
            settings.Alter<RabbitMqOptions>(s => s.Connections.Add(connectionName, connectionString));
        }

        /// <summary>
        /// Register configuration of the Rabbit MQ transport
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void ConfigureRabbitMq(this SettingsGraph settings, Action<RabbitMqOptions> configure)
        {
            settings.Alter(configure);
        }

        /// <summary>
        /// Register configuration of the Rabbit MQ transport
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void ConfigureRabbitMq(this SettingsGraph settings, Action<RabbitMqOptions, IHostEnvironment, IConfiguration> configure)
        {
            settings.Alter<RabbitMqOptions>((context, x) => configure(x, context.HostingEnvironment, context.Configuration));
        }
    }
}
