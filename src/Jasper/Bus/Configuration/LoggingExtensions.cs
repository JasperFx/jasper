using Jasper.Bus.Logging;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Bus.Configuration
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add a custom IBusLogger to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogBusEventsWith<T>(this Jasper.Configuration.Logging logging) where T : class, IBusLogger
        {
            logging.Parent.Services.AddTransient<IBusLogger, T>();
        }

        /// <summary>
        /// Add a custom IBusLogger to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="logger"></param>
        public static void LogBusEventsWith(this Jasper.Configuration.Logging logging, IBusLogger logger)
        {
            logging.Parent.Services.AddSingleton<IBusLogger>(logger);
        }

        /// <summary>
        /// Add a custom ITransportLogger to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogTransportEventsWith<T>(this Jasper.Configuration.Logging logging) where T : class, ITransportLogger
        {
            logging.Parent.Services.AddTransient<ITransportLogger, T>();
        }

        /// <summary>
        /// Add a custom ITransportLogger to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="logger"></param>
        public static void LogTransportEventsWith(this Jasper.Configuration.Logging logging, ITransportLogger logger)
        {
            logging.Parent.Services.AddSingleton<ITransportLogger>(logger);
        }

    }
}
