using Jasper.Bus.Logging;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Bus.Configuration
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add a custom IMessageLogger to the system for message level events
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogMessageEventsWith<T>(this Jasper.Configuration.Logging logging) where T : class, IMessageLogger
        {
            logging.Parent.Services.AddTransient<IMessageLogger, T>();
        }

        /// <summary>
        /// Add a custom IMessageLogger to the system for message level events
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="logger"></param>
        public static void LogMessageEventsWith(this Jasper.Configuration.Logging logging, IMessageLogger logger)
        {
            logging.Parent.Services.AddSingleton<IMessageLogger>(logger);
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
