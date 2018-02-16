using Jasper.Messaging.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Messaging.Configuration
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add a custom IExceptionSink to the system for logging exceptions
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogExceptionsWith<T>(this Jasper.Configuration.Logging logging) where T : class, IExceptionSink
        {
            logging.Parent.Services.AddTransient<IExceptionSink, T>();
        }

        /// <summary>
        /// Add a custom IExceptionSink to the system for logging exceptions
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="sink"></param>
        public static void LogExceptionsWith(this Jasper.Configuration.Logging logging, IExceptionSink sink)
        {
            logging.Parent.Services.AddSingleton(sink);
        }


        /// <summary>
        /// Add a custom IMessageEventSink to the system for message level events
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogMessageEventsWith<T>(this Jasper.Configuration.Logging logging) where T : class, IMessageEventSink
        {
            logging.Parent.Services.AddTransient<IMessageEventSink, T>();
        }

        /// <summary>
        /// Add a custom IMessageEventSink to the system for message level events
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="sink"></param>
        public static void LogMessageEventsWith(this Jasper.Configuration.Logging logging, IMessageEventSink sink)
        {
            logging.Parent.Services.AddSingleton<IMessageEventSink>(sink);
        }

        /// <summary>
        /// Add a custom ITransportEventSink to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <typeparam name="T"></typeparam>
        public static void LogTransportEventsWith<T>(this Jasper.Configuration.Logging logging) where T : class, ITransportEventSink
        {
            logging.Parent.Services.AddTransient<ITransportEventSink, T>();
        }

        /// <summary>
        /// Add a custom ITransportEventSink to the system
        /// </summary>
        /// <param name="logging"></param>
        /// <param name="sink"></param>
        public static void LogTransportEventsWith(this Jasper.Configuration.Logging logging, ITransportEventSink sink)
        {
            logging.Parent.Services.AddSingleton<ITransportEventSink>(sink);
        }

    }
}
