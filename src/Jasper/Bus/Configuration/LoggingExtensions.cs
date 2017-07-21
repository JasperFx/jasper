using Jasper.Bus.Logging;
using Jasper.Configuration;

namespace Jasper.Bus.Configuration
{
    public static class LoggingExtensions
    {
        public static void LogBusEventsWith<T>(this Jasper.Configuration.Logging logging) where T : IBusLogger
        {
            logging.Parent.Services.AddService<IBusLogger, T>();
        }

        public static void LogBusEventsWith(this Jasper.Configuration.Logging logging, IBusLogger logger)
        {
            logging.Parent.Services.AddService<IBusLogger>(logger);
        }

        public static void LogTransportEventsWith<T>(this Jasper.Configuration.Logging logging) where T : ITransportLogger
        {
            logging.Parent.Services.AddService<ITransportLogger, T>();
        }

        public static void LogTransportEventsWith(this Jasper.Configuration.Logging logging, ITransportLogger logger)
        {
            logging.Parent.Services.AddService<ITransportLogger>(logger);
        }
    }
}
