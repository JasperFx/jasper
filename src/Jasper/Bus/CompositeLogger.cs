using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Runtime;

namespace JasperBus
{
    public class CompositeLogger : IBusLogger
    {
        public CompositeLogger(IEnumerable<IBusLogger> loggers)
        {
            Loggers = loggers.ToArray();
        }

        public IBusLogger[] Loggers { get; }

        public void Sent(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.Sent(envelope);
            }
        }

        public void Received(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.Received(envelope);
            }
        }

        public void ExecutionStarted(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.ExecutionStarted(envelope);
            }
        }

        public void ExecutionFinished(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.ExecutionFinished(envelope);
            }
        }

        public void MessageSucceeded(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.MessageSucceeded(envelope);
            }
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            foreach (var sink in Loggers)
            {
                sink.MessageFailed(envelope, ex);
            }
        }

        public void LogException(Exception ex, string correlationId = null, string message = null)
        {
            foreach (var sink in Loggers)
            {
                sink.LogException(ex, correlationId, message);
            }
        }

        public void NoHandlerFor(Envelope envelope)
        {
            foreach (var sink in Loggers)
            {
                sink.NoHandlerFor(envelope);
            }
        }
    }
}