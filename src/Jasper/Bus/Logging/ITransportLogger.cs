using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Bus.Logging
{
    [Obsolete("Not used anywhere")]
    public interface ITransportLogger
    {
        void Debug(string message);
        void Info(string message);
        void Error(string message, Exception exception);
    }

    public class TransportLogger : ITransportLogger
    {
        public static ITransportLogger Combine(ITransportLogger[] loggers)
        {
            switch (loggers.Length)
            {
                case 0:
                    return new TransportLogger();
                case 1:
                    return loggers[0];
                default:
                    return new CompositeTransportLogger(loggers);
            }
        }

        public virtual void Debug(string message)
        {
        }

        public virtual void Error(string message, Exception exception)
        {
        }

        public virtual void Info(string message)
        {
        }
    }

    public class ConsoleTransportLogger : ITransportLogger
    {
        public void Debug(string message)
        {
            Console.WriteLine($"Transport DEBUG: {message}");
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine($"Transport ERROR: {message}\n{exception}\n");
        }

        public void Info(string message)
        {
            Console.WriteLine($"Transport INFO: {message}");
        }
    }

    public class CompositeTransportLogger : ITransportLogger
    {
        public CompositeTransportLogger(IEnumerable<ITransportLogger> loggers)
        {
            Loggers = loggers.ToArray();
        }

        public ITransportLogger[] Loggers { get; }

        public void Debug(string message)
        {
            foreach(var sink in Loggers)
            {
                sink.Debug(message);
            }
        }

        public void Error(string message, Exception exception)
        {
            foreach(var sink in Loggers)
            {
                sink.Error(message, exception);
            }
        }

        public void Info(string message)
        {
            foreach(var sink in Loggers)
            {
                sink.Info(message);
            }
        }
    }
}
