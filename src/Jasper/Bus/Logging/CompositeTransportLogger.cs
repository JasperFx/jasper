using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Logging
{
    public class CompositeTransportLogger : ITransportLogger
    {
        public ITransportLogger[] Loggers { get; }

        public CompositeTransportLogger(ITransportLogger[] loggers)
        {
            Loggers = loggers;
        }

        public void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.OutgoingBatchSucceeded(batch);
                }
                catch (Exception)
                {

                }
            }
        }

        public void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.OutgoingBatchFailed(batch, ex);
                }
                catch (Exception)
                {

                }
            }
        }

        public void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.IncomingBatchReceived(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void CircuitBroken(Uri destination)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.CircuitBroken(destination);
                }
                catch (Exception)
                {

                }
            }
        }

        public void CircuitResumed(Uri destination)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.CircuitResumed(destination);
                }
                catch (Exception)
                {

                }
            }
        }

        public void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.ScheduledJobsQueuedForExecution(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.RecoveredIncoming(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.RecoveredOutgoing(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.DiscardedExpired(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.LogException(ex, correlationId, message);
                }
                catch (Exception)
                {

                }
            }
        }

        public static CompositeTransportLogger Empty()
        {
            return new CompositeTransportLogger(new ITransportLogger[0]);
        }

        public void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    logger.DiscardedUnknownTransport(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
