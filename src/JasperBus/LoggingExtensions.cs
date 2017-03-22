using Baseline;
using Jasper;

namespace JasperBus
{
    public static class LoggingExtensions
    {
        public static void LogBusEventsWith<T>(this Logging logging) where T : IBusLogger
        {
            logging.As<ILogging>().Parent.Services.AddService<IBusLogger, T>();
        }

        public static void LogBusEventsWith(this Logging logging, IBusLogger logger)
        {
            logging.As<ILogging>().Parent.Services.AddService<IBusLogger>(logger);
        }
    }
}