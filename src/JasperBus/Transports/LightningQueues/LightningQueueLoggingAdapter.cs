using System;
using JasperBus.Queues.Logging;

namespace JasperBus.Transports.LightningQueues
{
    public class LightningQueueLoggingAdapter : ILogger
    {
        private readonly ITransportLogger _logger;

        public LightningQueueLoggingAdapter(ITransportLogger logger)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void DebugFormat(string message, params object[] args)
        {
            _logger.Debug(string.Format(message, args));
        }

        public void DebugFormat(string message, object arg1, object arg2)
        {
            _logger.Debug(string.Format(message, arg1, arg2));
        }

        public void DebugFormat(string message, object arg1)
        {
            _logger.Debug(string.Format(message, arg1));
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void InfoFormat(string message, params object[] args)
        {
            _logger.Info(string.Format(message, args));
        }

        public void InfoFormat(string message, object arg1, object arg2)
        {
            _logger.Info(string.Format(message, arg1, arg2));
        }

        public void InfoFormat(string message, object arg1)
        {
            _logger.Info(string.Format(message, arg1));
        }
    }
}
